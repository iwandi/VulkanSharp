using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Vulkan;

// TODO : vulkan
//          - Flag enum same type as in api QueueFlags is int but in api is uint
//          - Flag enum with null Entry
//          - Use enum in api
//          - PhysicalDeviceMemoryProperties.MemoryTpyes is not a array
//          - PhysicalDeviceMemoryProperties.MemoryHeapCount is not a array
//          - DeviceCreateInfo.QueueCreateInfoCount shuld not be exposed


namespace vulkaninfo
{
    public class InfoGenerator
    {
        public string ApplicationName { get; set; }
        public uint ApplicationVersion { get; set; }
        public string EngineName { get; set; }
        public uint EnginveVersion { get; set; }
        public string[] KnownExtensions { get; set; }
        public string[] KnownDeviceExtensions { get; set; }

        public InfoGenerator()
        {
            ApplicationName = "vulkaninfo";
            ApplicationVersion = 1;
            EngineName = ApplicationName;
            EnginveVersion = ApplicationVersion;
            KnownExtensions = new string[]
            {
                "VK_KHR_surface",
#if VK_USE_PLATFORM_ANDROID_KHR
        "VK_KHR_android_surface",
#endif
#if VK_USE_PLATFORM_MIR_KHR
        "VK_KHR_mir_surface",
#endif
#if VK_USE_PLATFORM_WAYLAND_KHR
        "VK_KHR_wayland_surface",
#endif
#if VK_USE_PLATFORM_WIN32_KHR
        "VK_KHR_win32_surface",
#endif
#if VK_USE_PLATFORM_XCB_KHR
        "VK_KHR_xcb_surface",
#endif
#if VK_USE_PLATFORM_XLIB_KHR
        "VK_KHR_xlib_surface",
#endif
            };
            KnownDeviceExtensions = new string[]
            {
                "VK_KHR_swapchain"
            };
        }

        void AppDevInitFormats(AppDev dev)
        {
            // TODO
        }

        void ExtractVersion(uint version, out uint major, out uint minor, out uint patch)
        {
            major = version >> 22;
            minor = version >> 11 & 0x3ff;
            patch = version & 0xfff;
        }

        AppDev AppDevInit(AppGpu gpu)
        {
            DeviceCreateInfo info = new DeviceCreateInfo
            {
                QueueCreateInfoCount = 0, // TODO : this sould not be 
                QueueCreateInfos = new DeviceQueueCreateInfo[0],
                EnabledLayerCount = 0,
                EnabledLayerNames = new string[0],
                EnabledExtensionCount = 0,
                EnabledExtensionNames = new string[0],
            };

            // Scan layers

            // TODO

            Device device = gpu.Obj.CreateDevice(info, null);

            return new AppDev
            {
                Obj = device,
            };
        }
        
        void AppDevDestroy(AppDev dev)
        {
            dev.Obj.Destroy(null);
        }

        AppInstance AppCreateInstance(uint apiVersion)
        {
            ApplicationInfo appInfo = new ApplicationInfo
            {
                ApplicationName = ApplicationName,
                ApplicationVersion = ApplicationVersion,
                EngineName = EngineName,
                EngineVersion = EnginveVersion,
                ApiVersion = apiVersion,
            };

            InstanceCreateInfo createInfo = new InstanceCreateInfo
            {
                ApplicationInfo = appInfo,
                EnabledLayerCount = 0,
                EnabledExtensionCount = 0,
            };

            // Scan layers
            List<LayerExtensionList> layers = new List<LayerExtensionList>();
            foreach (LayerProperties layer in Commands.EnumerateInstanceLayerProperties())
            {
                LayerExtensionList layerExtList = new LayerExtensionList
                {
                    LayerProperties = layer,
                    ExtensionProperties = Commands.EnumerateInstanceExtensionProperties(layer.LayerName),
                };
                if(layerExtList.ExtensionProperties == null)
                {
                    layerExtList.ExtensionProperties = new ExtensionProperties[0];
                }
                layers.Add(layerExtList);
            }

            ExtensionProperties[] extensions = Commands.EnumerateInstanceExtensionProperties("");

            foreach (string knownExtName in KnownExtensions)
            {
                bool extensionFound = false;
                foreach (ExtensionProperties extention in extensions)
                {
                    if (extention.ExtensionName == knownExtName)
                    {
                        extensionFound = true;
                        break;
                    }
                }

                if (!extensionFound)
                {
                    throw new Exception("Cannot find extension: " + knownExtName);
                }
            }

            createInfo.EnabledExtensionNames = KnownExtensions;
            createInfo.EnabledExtensionCount = (uint)KnownExtensions.Length;

            // TODO : Register debug callback

            Instance instance = new Instance(createInfo);

            return new AppInstance
            {
                Instance = instance,
                Layers = layers.ToArray(),
                Extensions = extensions,
            };
        }

        void AppDestroyInstance(AppInstance instance)
        {
            // TODO : Check if we need to free some structs

            instance.Instance.Destroy(null);
        }

        AppGpu AppGpuInit(uint id, PhysicalDevice obj)
        {
            AppGpu gpu = new AppGpu
            {
                Id = id,
                Obj = obj,
                Props = obj.GetProperties(),
                QueueProps = obj.GetQueueFamilyProperties(),
                MemoryProps = obj.GetMemoryProperties(),
                Features = obj.GetFeatures(),
            };

            gpu.QueueReqs = new DeviceQueueCreateInfo[gpu.QueueProps.Length];
            for (uint i = 0; i < gpu.QueueProps.Length; i++)
            {
                uint queueCount = gpu.QueueProps[i].QueueCount;
                gpu.QueueReqs[i] = new DeviceQueueCreateInfo
                { 
                    QueueFamilyIndex = i,
                    QueueCount = queueCount,
                    QueuePriorities = new float[queueCount],
                };
            }

            gpu.Device = AppDevInit(gpu);            
            AppDevInitFormats(gpu.Device);

            return gpu;
        }

        void AppGpuDestroy(AppGpu gpu)
        {
            AppDevDestroy(gpu.Device);

            // TODO : Check if we need to free some structs
        }

        void AppDevDumpFormatProps(AppDev dev, Format fmt, StreamWriter output)
        {
            // TODO 
        }

        void AppDevDump(AppDev dev, StreamWriter output)
        {
            // TODO
        }

        void AppGpuDumpFeatures(AppGpu gpu, StreamWriter output)
        {
            // TODO
        }

        void AppDumpSparseProps(PhysicalDeviceSparseProperties sparseProps, StreamWriter output)
        {
            // TODO
        }

        void AppDumpLimits(PhysicalDeviceLimits limits, StreamWriter output)
        {
            // TODO 
        }

        void AppGpuDumpProps(AppGpu gpu, StreamWriter output)
        {
            // TODO
        }

        void AppDumpExtensions(string ident, string layerName, ExtensionProperties[] extensionProperties, StreamWriter output)
        {
            if (!string.IsNullOrEmpty(layerName))
            {
                output.WriteLine("{0}{1} Extensions", ident, layerName);
            }
            else
            {
                output.WriteLine("Extensions");
            }

            output.WriteLine("\tcount = {0}", extensionProperties.Length);
            foreach (ExtensionProperties extProp in extensionProperties)
            {
                output.Write("{0}\t", ident);
                output.WriteLine("{0,-32}: extension revision {1}", extProp.ExtensionName, extProp.SpecVersion);
            }
        }
        
        void AppGpuDumpQueuProps(AppGpu gpu, uint id, StreamWriter output)
        {
            QueueFamilyProperties props = gpu.QueueProps[id];

            output.WriteLine("VkQueueFamilyProperties[{0}]:", id);
            output.WriteLine("============================");
            output.WriteLine("\tqueueFlags         = {0}{1}{2}{3}",
                   ((QueueFlags)props.QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics ? 'G' : '.',
                   ((QueueFlags)props.QueueFlags & QueueFlags.Compute) == QueueFlags.Compute ? 'C' : '.', 
                   ((QueueFlags)props.QueueFlags & QueueFlags.Transfer) == QueueFlags.Transfer ? 'D' : '.',
                   ((QueueFlags)props.QueueFlags & QueueFlags.SparseBinding) == QueueFlags.SparseBinding ? 'S' : '.'); // Add this option, not pressent in original
            output.WriteLine("\tqueueCount         = {0}", props.QueueCount);
            output.WriteLine("\ttimestampValidBits = {0}", props.TimestampValidBits);
            output.WriteLine("\tminImageTransferGranularity = ({0}, {1}, {2})",
                   props.MinImageTransferGranularity.Width,
                   props.MinImageTransferGranularity.Height,
                   props.MinImageTransferGranularity.Depth);
        }

        void AppGpuDumpMemoryProps(AppGpu gpu, StreamWriter output)
        {
            PhysicalDeviceMemoryProperties props = gpu.MemoryProps;

            output.WriteLine("VkPhysicalDeviceMemoryProperties:");
            output.WriteLine("=================================");
            output.WriteLine("\tmemoryTypeCount       = {0}", props.MemoryTypeCount);
            // TODO : loop this 
            if (props.MemoryTypeCount > 0)
            {
                int i = 0;
                MemoryType memoryType = props.MemoryTypes;

                output.WriteLine("\tmemoryTypes[{0}] : ", i);
                output.WriteLine("\t\tpropertyFlags = {0}", memoryType.PropertyFlags);
                output.WriteLine("\t\theapIndex     = {0}", memoryType.HeapIndex);
            }
            output.WriteLine("\tmemoryHeapCount       = {0}", props.MemoryHeapCount);
            // TODO : loop this 
            if(props.MemoryHeapCount > 0)
            {
                int i = 0;
                MemoryHeap memoryHeap = props.MemoryHeaps;

                output.WriteLine("\tmemoryHeaps[{0}] : ", i);
                output.WriteLine("\t\tsize          = {0}", memoryHeap.Size);
            }
        }

        void AppGpuDump(AppGpu gpu, StreamWriter output)
        {
            output.WriteLine("Device Extensions and layers:");
            output.WriteLine("=============================");
            output.WriteLine("GPU{0}", gpu.Id);
            AppGpuDumpProps(gpu, output);
            output.WriteLine();
            AppDumpExtensions("", "Device", gpu.DeviceExtensions, output);

            output.WriteLine();
            output.WriteLine("Layers\tcount = {0}", gpu.DeviceLayers.Length);
            foreach (LayerExtensionList layerInfo in gpu.DeviceLayers)
            {
                uint major, minor, patch;

                ExtractVersion(layerInfo.LayerProperties.SpecVersion, out major, out minor, out patch);
                string specVersion = string.Format("{0}.{1}.{2}", major, minor, patch);
                string layerVersion = string.Format("{0}", layerInfo.LayerProperties.ImplementationVersion);

                output.WriteLine("\t%s (%s) Vulkan version %s, layer version %s",
                   layerInfo.LayerProperties.LayerName,
                   layerInfo.LayerProperties.Description, specVersion,
                   layerVersion);

                AppDumpExtensions("\t", layerInfo.LayerProperties.LayerName, layerInfo.ExtensionProperties, output);
            }

            output.WriteLine();
            // TODO : make this loop
            //foreach(var a in gpu.QueueFamilyProperties)
            /*for (i = 0; i < gpu.queue_count; i++)
            {
                AppGpuDumpQueuProps(gpu, i, output);
                output.WriteLine();
            }*/

            AppGpuDumpMemoryProps(gpu, output);
            output.WriteLine();
            AppGpuDumpFeatures(gpu, output);
            output.WriteLine();
            AppDevDump(gpu.Device, output);
        }

        public void DumpInfo(StreamWriter output)
        {
            uint apiVersion = Vulkan.Version.Make(1, 0, 0);

            DumpHeader(apiVersion, output);

            AppInstance instance = AppCreateInstance(apiVersion);
            output.WriteLine("Instance Extensions and layers:");
            output.WriteLine("===============================");
            AppDumpExtensions("", "Instance", instance.Extensions, output);

            output.WriteLine("Instance Layers\tcount = {0}", instance.Layers.Length);
            foreach (LayerExtensionList layer in instance.Layers)
            {
                LayerProperties layerProp = layer.LayerProperties;

                uint major, minor, patch;

                ExtractVersion(layerProp.SpecVersion, out major, out minor, out patch);
                string specVersion = string.Format("{0}.{1}.{2}", major, minor, patch);
                string layerVersion = string.Format("{0}", layerProp.ImplementationVersion);

                output.WriteLine("\t{0} ({1}) Vulkan version {2}, layer version {3}",
                    layerProp.LayerName, layerProp.Description,
                    specVersion, layerVersion);

                AppDumpExtensions("\t", layerProp.LayerName, layer.ExtensionProperties, output);
            }
            
            PhysicalDevice[] objs = instance.Instance.EnumeratePhysicalDevices();
            AppGpu[] gpus = new AppGpu[objs.Length];

            for(uint i = 0; i < objs.Length; i++)
            {
                gpus[i] = AppGpuInit(i, objs[i]);
                AppGpuDump(gpus[i], output);
                output.WriteLine();
                output.WriteLine();
            }

            for (uint i = 0; i < gpus.Length; i++)
            {
                AppGpuDestroy(gpus[i]);
            }

            AppDestroyInstance(instance);
            output.Flush();
        }

        void DumpHeader(uint apiVersion, StreamWriter output)
        {
            uint major, minor, patch;

            ExtractVersion(apiVersion, out major, out minor, out patch);

            output.WriteLine("===========");
            output.WriteLine("VULKAN INFO");
            output.WriteLine("===========\n");
            output.WriteLine("Vulkan API Version: {0}.{1}.{2}\n", major, minor, patch);
        }

        class AppInstance // app_instance 
        {
            public Instance Instance;
            public LayerExtensionList[] Layers;
            public ExtensionProperties[] Extensions;
        }

        class LayerExtensionList // layer_extension_list 
        {
            public LayerProperties LayerProperties;
            public ExtensionProperties[] ExtensionProperties;
        }

        class AppGpu // app_gpu 
        {
            public uint Id;
            public PhysicalDevice Obj;
            public PhysicalDeviceProperties Props;

            public QueueFamilyProperties[] QueueProps;
            public DeviceQueueCreateInfo[] QueueReqs;

            public PhysicalDeviceMemoryProperties MemoryProps;
            public PhysicalDeviceFeatures Features;
            public PhysicalDeviceLimits Limits;

            public LayerExtensionList[] DeviceLayers;
            public ExtensionProperties[] DeviceExtensions;

            public AppDev Device;
        }

        class AppDev // app_dev 
        {
            public AppGpu Gpu;
            public Device Obj;
            FormatProperties[] FormatProbs; /*VK_FORMAT_RANGE_SIZE*/
        }
    }
}
