using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Vulkan;

namespace vulkaninfo
{
    public class InfoGenerator
    {
        public string ApplicationName { get; set; }
        public uint ApplicationVersion { get; set; }
        public string EngineName { get; set; }
        public uint EnginveVersion { get; set; }
        public string[] KnownExtensions { get; set; }

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
        }

        Gloabl CreateInstance(uint apiVersion)
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
            LayerProperties[]  layerProperties = Commands.EnumerateInstanceLayerProperties();

            // Collect global extensions
            List<ExtensionProperties> extentions = new List<ExtensionProperties>();
            foreach(LayerProperties layerProp in layerProperties)
            {
                ExtensionProperties[] layerExtentionProp = Commands.EnumerateInstanceExtensionProperties(layerProp.LayerName);
                if(layerExtentionProp != null)
                {
                    extentions.AddRange(layerExtentionProp);
                }
            }
            ExtensionProperties[] extensionProperties = extentions.ToArray();
            // TODO : validate the KnownExtention agianst this resault

            // TODO : this failes
            createInfo.EnabledExtensionNames = KnownExtensions;
            //createInfo.EnabledExtensionCount = (uint)KnownExtensions.Length;

            Instance instance = new Instance(createInfo);

            return new Gloabl
            {
                Instance = instance,
                LayerProperties = layerProperties,
                ExtensionProperties = extensionProperties,
            };
        }

        public void WriteInfo(StreamWriter output)
        {
            uint apiVersion = Vulkan.Version.Make(1, 0, 0);

            WriteHeader(apiVersion, output);
            
            Gloabl gloabl = CreateInstance(apiVersion);
            WriteExtensionInfo("", "Instance", gloabl.ExtensionProperties, output);

            output.WriteLine("Instance Layers\tcount = {0}", gloabl.LayerProperties.Length);
            foreach(LayerProperties layer in gloabl.LayerProperties)
            {

            }

            output.Flush();
        }

        public void WriteExtensionInfo(string ident, string layerName, ExtensionProperties[] properties, StreamWriter output)
        {

        }

        void WriteHeader(uint apiVersion, StreamWriter output)
        {
            uint major = apiVersion >> 22;
            uint minor = apiVersion >> 11 & 0x3ff;
            uint patch = apiVersion & 0xfff;

            output.WriteLine("===========");
            output.WriteLine("VULKAN INFO");
            output.WriteLine("===========\n");
            output.WriteLine("Vulkan API Version: {0}.{1}.{2}\n", major, minor, patch);


        }

        class Gloabl
        {
            public Instance Instance;
            public LayerProperties[] LayerProperties;
            public ExtensionProperties[] ExtensionProperties;
        }
    }
}
