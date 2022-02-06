using EpsilonLib.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;
using TagTool.Commands.Editing;
using TagTool.IO;
using TagTool.Serialization;

namespace CacheEditor.RTE.Providers
{
    [Export(typeof(IRteProvider))]
    class HaloOnline106708Provider : IRteProvider
    {
        public IEnumerable<IRteTarget> FindTargets()
        {
            foreach (var process in Process.GetProcessesByName("eldorado"))
            {
                if (PCProcessTarget.TryCreate(this, process, out PCProcessTarget target))
                    yield return target;
            }
        }

        public bool ValidForCacheFile(ICacheFile cacheFile)
        {
            return cacheFile.Cache is GameCacheHaloOnlineBase;
        }

        void IRteProvider.PokeTag(IRteTarget target, GameCache cache, CachedTag instance, object definition, ref byte[] RuntimeTagData)
        {
            PokeTag(target, cache, definition, instance as CachedTagHaloOnline, ref RuntimeTagData);
        }

        private void PokeTag(IRteTarget target, GameCache cache, object definition, CachedTagHaloOnline hoInstance, ref byte[] RuntimeTagDataMap)
        {
            var process = Process.GetProcessById((int)target.Id);
            if (process == null)
                throw new RteTargetNotAvailableException(this, "Target process could not be found.");

            using (var processStream = new ProcessMemoryStream(process))
            {
                int tagindex = hoInstance.Index;
                bool isModPackage = false;
                //bool isModPackageTag = false;
                GameCacheModPackage modpak = null;
                if (cache is GameCacheModPackage)
                {
                    modpak = (GameCacheModPackage)cache;
                    isModPackage = true;
                }                 

                #if DEBUG
                {
                    if (isModPackage && 
                        !modpak.BaseCacheReference.TagCache.TryGetCachedTag(hoInstance.Index, out var baseTag))
                    {
                        int paktagcount = 0;
                        for (var i = 0; i < tagindex; i++)
                            if (modpak.TagCache.TryGetCachedTag(i, out var taginstance) && !((CachedTagHaloOnline)taginstance).IsEmpty())
                                paktagcount++;
                        tagindex = 0xFFFE - paktagcount;
                        //isModPackageTag = true;
                    }
                }
                #endif

                var address = GetTagAddress(processStream, tagindex);
                if (address == 0)
                    throw new RteProviderException(this, $"Tag '{hoInstance}' could not be located in the target process.");

                //first get a raw copy of the tag in the cache
                byte[] tagcachedata;
                using (var stream = cache.OpenCacheRead())
                using (var outstream = new MemoryStream())
                using (EndianWriter writer = new EndianWriter(outstream, EndianFormat.LittleEndian))
                {
                    //deserialize the cache def then reserialize to a stream
                    var cachedef = cache.Deserialize(stream, hoInstance);
                    var dataContext = new DataSerializationContext(writer);
                    cache.Serializer.Serialize(dataContext, cachedef);
                    StreamUtil.Align(outstream, 0x10);
                    tagcachedata = outstream.ToArray();
                }

                //then serialize the current version of the tag in the editor
                byte[] editordata;
                using (MemoryStream stream = new MemoryStream())
                using (EndianWriter writer = new EndianWriter(stream, EndianFormat.LittleEndian))
                {
                    var dataContext = new DataSerializationContext(writer);
                    cache.Serializer.Serialize(dataContext, definition);
                    StreamUtil.Align(stream, 0x10);
                    editordata = stream.ToArray();
                }

                //length should make to make sure the serializer is consistent
                if (tagcachedata.Length != editordata.Length)
                {
                    throw new RteProviderException(this, $"Error: tag size changed or the serializer failed!");
                }

                //some very rare tags have a size that doesn't match our serialized version, need to fix root cause
                if (!isModPackage && tagcachedata.Length != hoInstance.TotalSize - hoInstance.CalculateHeaderSize())
                {
                    throw new RteProviderException(this, $"Sorry can't poke this specific tag yet (only happens with very rare specific tags), go bug a dev");
                }

                //pause the process during poking to prevent race conditions
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                process.Suspend();

                uint currenttotalsize = 0;
                uint headersize = 0;
                List<uint> TagReferenceFixups = new List<uint>();
                using (EndianReader reader = new EndianReader(processStream))
                {
                    CachedTagHaloOnline runtimetag = new CachedTagHaloOnline();
                    processStream.Seek(address + 4, SeekOrigin.Begin);
                    ReadHeaderValues(reader, ref currenttotalsize, ref headersize, ref TagReferenceFixups);
                }

                byte[] CurrentRuntimeTagData = new byte[currenttotalsize - headersize];
                processStream.Seek(address + headersize, SeekOrigin.Begin);
                processStream.Read(CurrentRuntimeTagData, 0, (int)(currenttotalsize - headersize));

                if (currenttotalsize - headersize != tagcachedata.Length)
                {
                    process.Resume();
                    throw new RteProviderException(this, $"Error: Loaded tag size did not match cache tag size. Is this tag overwritten in a modpak? Is your modpak built on the most up to date cache?");
                }

                //Store the process data before the first poke so we know which values are runtime values
                if (RuntimeTagDataMap.Length == 0)
                {
                    RuntimeTagDataMap = new byte[CurrentRuntimeTagData.Length];
                    for(var i = 0; i < CurrentRuntimeTagData.Length; i++)
                    {
                        //this will serve as a map of the tag data, with 1 being pokeable fields, and 0 being nonpokeable
                        if (CurrentRuntimeTagData[i] == tagcachedata[i])
                            RuntimeTagDataMap[i] = 1;
                    }
                }

                if (tagcachedata.Length != RuntimeTagDataMap.Length)
                {
                    process.Resume();
                    throw new RteProviderException(this, $"Error: Loaded tag has changed size since initial poke! Try closing and reopening the tag.");
                }

                //fixup modpak tagrefs
                #if DEBUG
                if(isModPackage)
                {
                    //a list of all modpak tags in the modpak that are not basecache tags
                    List<int> modpaktagindices = new List<int>();
                    for (var i = 0; i < modpak.TagCache.Count; i++)
                        if (modpak.TagCache.TryGetCachedTag(i, out var taginstance) && !((CachedTagHaloOnline)taginstance).IsEmpty())
                            modpaktagindices.Add(i);

                    foreach (uint tagreffixup in TagReferenceFixups)
                    {
                        int editortagref = BitConverter.ToInt32(editordata, (int)(tagreffixup - headersize));

                        //patching all tagrefs by default because they are runtime fields and we need to be able to reset them to default values
                        if (!modpak.BaseCacheReference.TagCache.TryGetCachedTag(editortagref, out var baseTag))
                        {
                            //find the index of our desired tag in relation to all modpak tags in the modpak that are not basecache tags
                            int paktagcount = modpaktagindices.Count(x => x < editortagref);
                            editortagref = 0xFFFE - paktagcount;

                            byte[] newvalue = BitConverter.GetBytes(editortagref);
                            newvalue.CopyTo(CurrentRuntimeTagData, tagreffixup - headersize);
                        }
                    }
                }
                #endif

                //write diffed bytes only
                int patchedbytes = 0;
                for (var i = 0; i < tagcachedata.Length; i++)
                {
                    //patch anything that isn't a runtime modified field
                    if(RuntimeTagDataMap[i] == 1)
                    {
                        CurrentRuntimeTagData[i] = editordata[i];
                        patchedbytes++;
                    }
                }

                processStream.Seek(address + headersize, SeekOrigin.Begin);
                processStream.Write(CurrentRuntimeTagData, 0, CurrentRuntimeTagData.Length);
                processStream.Flush();

                process.Resume();
                stopWatch.Stop();

                Console.WriteLine($"Patched {patchedbytes} bytes in {stopWatch.ElapsedMilliseconds / 1000.0f} seconds");
            }
        }

        private void ReadHeaderValues(EndianReader reader, ref uint currenttotalsize, ref uint headersize, ref List<uint> TagReferenceFixups)
        {
            currenttotalsize = reader.ReadUInt32();
            var numDependencies = reader.ReadInt16();              // 0x08 int16  dependencies count
            var numDataFixups = reader.ReadInt16();                // 0x0A int16  data fixup count
            var numResourceFixups = reader.ReadInt16();            // 0x0C int16  resource fixup count
            var numTagReferenceFixups = reader.ReadInt16();        // 0x0E int16  tag reference fixup count(was padding)

            reader.BaseStream.Position += 20; //skip to dependencies section
            reader.BaseStream.Position += 4 * numDependencies; //skip dependencies
            reader.BaseStream.Position += 4 * numDataFixups; //skip datafixups
            reader.BaseStream.Position += 4 * numResourceFixups; //skip resourcefixups
            for (var i = 0; i < numTagReferenceFixups; i++)
                TagReferenceFixups.Add(reader.ReadUInt32() - 0x40000000);

            headersize = CalculateMemoryHeaderSize(0x24, numDependencies, numDataFixups, numResourceFixups, numTagReferenceFixups);
        }
        public uint CalculateMemoryHeaderSize(int TagHeaderSize, int numDependencies, int numDataFixups, int numResourceFixups, int numTagReferenceFixups)
        {
            var size = (uint)(TagHeaderSize + numDependencies * 4 + numDataFixups * 4 + numResourceFixups * 4 + numTagReferenceFixups * 4);
            return (uint)((size + 0xF) & ~0xF);  // align to 0x10
        }

        private static uint GetTagAddress(ProcessMemoryStream stream, int tagIndex)
        {
            // Read the tag count and validate the tag index
            var reader = new BinaryReader(stream);
            reader.BaseStream.Position = 0x22AB008;
            var maxIndex = 0xFFFF;
            if (tagIndex >= maxIndex)
                return 0;

            // Read the tag index table to get the index of the tag in the address table
            reader.BaseStream.Position = 0x22AAFFC;
            var tagIndexTableAddress = reader.ReadUInt32();
            if (tagIndexTableAddress == 0)
                return 0;
            reader.BaseStream.Position = tagIndexTableAddress + tagIndex * 4;
            var addressIndex = reader.ReadInt32();
            if (addressIndex < 0)
                return 0;

            // Read the tag's address in the address table
            reader.BaseStream.Position = 0x22AAFF8;
            var tagAddressTableAddress = reader.ReadUInt32();
            if (tagAddressTableAddress == 0)
                return 0;
            reader.BaseStream.Position = tagAddressTableAddress + addressIndex * 4;
            return reader.ReadUInt32();
        }

        public void DumpData(string filename, byte[] data)
        {
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }
    }
}
