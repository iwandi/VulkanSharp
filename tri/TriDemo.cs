using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vulkan;
using Vulkan.Windows;

namespace tri
{
    public class TriDemo
    {
        const string AppName = "tri";
        const uint AppVersion = 1;

        readonly string[] InstanceValidationLayersAlt1 = {
            "VK_LAYER_LUNARG_standard_validation",
        };

        readonly string[] InstanceValidationLayersAlt2 = {
            "VK_LAYER_GOOGLE_threading",     "VK_LAYER_LUNARG_parameter_validation",
            "VK_LAYER_LUNARG_device_limits", "VK_LAYER_LUNARG_object_tracker",
            "VK_LAYER_LUNARG_image",         "VK_LAYER_LUNARG_core_validation",
            "VK_LAYER_LUNARG_swapchain",     "VK_LAYER_GOOGLE_unique_objects"
        };

        readonly string[] InstanceExtentions =
        {
            "VK_KHR_surface",
            "VK_KHR_win32_surface",
        };

        readonly string[] DeviceExtentions =
        {
            "VK_KHR_swapchain",
        };

        public void RunDemo()
        {
            RenderSystem renderSys = new RenderSystem();
            if(!renderSys.TryInit(AppName, AppVersion, InstanceValidationLayersAlt1, InstanceExtentions))
            {
                renderSys.TryInit(AppName, AppVersion, InstanceValidationLayersAlt2, InstanceExtentions);
            }
            renderSys.TryCreateDevice(DeviceExtentions);
            renderSys.CreateWindow();
            renderSys.CreateSwapChain();
            
            Prepare(renderSys);
            Run(renderSys);

            renderSys.ShutDown();
        }

        void Prepare(RenderSystem renderSys)
        {

        }

        void Run(RenderSystem renderSys)
        {

        }
    }
}
