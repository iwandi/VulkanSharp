using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vulkan;
using Vulkan.Windows;

namespace tri
{
    public class RenderSystem
    {
        readonly uint ApiVersion = Vulkan.Version.Make(1, 0, 0);

        public Instance Instance;
        PhysicalDevice[] gpuList;


        bool Validate<T>(string[] names, T[] list, Func<T,string, bool> check)
        {
            foreach (string name in names)
            {
                bool found = false;
                foreach (T element in list)
                {
                    if (check(element, name))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        bool ValidateLayers(string[] names, LayerProperties[] list)
        {
            return Validate<LayerProperties>(names, list, (element, name) =>
            {
                return element.LayerName == name;
            });
        }

        bool ValidateExtentsions(string[] names, ExtensionProperties[] list)
        {
            return Validate<ExtensionProperties>(names, list, (element, name) =>
            {
                return element.ExtensionName == name;
            });
        }

        public bool TryInit(string appName, uint version, string[] validationLayers, string[] extensionNames)
        {
            LayerProperties[] layers = Commands.EnumerateInstanceLayerProperties();
            if(!ValidateLayers(validationLayers, layers))
            {
                return false;
            }

            ExtensionProperties[] extensions = Commands.EnumerateInstanceExtensionProperties("");
            if(!ValidateExtentsions(extensionNames,extensions))
            {
                return false;
            }

            InstanceCreateInfo info = new InstanceCreateInfo
            {
                ApplicationInfo = new ApplicationInfo
                {
                    ApplicationName = appName,
                    ApplicationVersion = 0,
                    EngineName = appName,
                    EngineVersion = 0,
                    ApiVersion = ApiVersion,
                },
                EnabledLayerNames = validationLayers,
                EnabledExtensionNames = extensionNames,
            };

            Instance = new Instance(info);

            gpuList = Instance.EnumeratePhysicalDevices();
            if(gpuList.Length < 0)
            {
                return false;
            }

            return true;
        }

        public bool TryCreateDevice(string[] extensionNames)
        {
            return false;
        }

        public void CreateWindow()
        {

        }

        public void CreateSwapChain()
        {

        }

        public void ShutDown()
        {
            Instance.Destroy(null);
        }
    }
}
