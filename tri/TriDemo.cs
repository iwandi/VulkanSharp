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
        const uint WindowWidth = 800;
        const uint WindowHeight = 600;

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

        bool run;

        CommandBuffer cmdDraw;

        public void RunDemo()
        {
            RenderSystem renderSys = new RenderSystem();
            renderSys.WindowClosed += RenderWindowClosed;

            if (!renderSys.TryInit(AppName, AppVersion, InstanceValidationLayersAlt1, InstanceExtentions))
            {
                renderSys.TryInit(AppName, AppVersion, InstanceValidationLayersAlt2, InstanceExtentions);
            }
            renderSys.TryCreateDevice(DeviceExtentions);
            renderSys.CreateWindow(WindowWidth, WindowHeight);
            renderSys.CreateSwapChain(PresentModeKhr.Fifo, 1);
            renderSys.CreateDepth();

            Prepare(renderSys);

            run = true;
            while (run)
            {
                renderSys.HandleEvents();

                RenderFrame(renderSys);

                renderSys.Present();
            }

            renderSys.ShutDown();
        }

        void Prepare(RenderSystem renderSys)
        {
            cmdDraw = renderSys.CreateCommandBuffer(QueueFlags.Graphics);

            PrepareTextures(renderSys);
            PrepareVertices(renderSys);
            PrepareDescriptorLayout(renderSys);
            PrepareRenderPass(renderSys);
            PreparePipeline(renderSys);
            PrepareDescriptorPool(renderSys);
            PrepareDescriptorSet(renderSys);
            PrepareFrameBuffer(renderSys);
        }

        void PrepareTextures(RenderSystem renderSys)
        {

        }

        void PrepareVertices(RenderSystem renderSys)
        {

        }

        void PrepareDescriptorLayout(RenderSystem renderSys)
        {

        }

        void PrepareRenderPass(RenderSystem renderSys)
        {

        }

        void PreparePipeline(RenderSystem renderSys)
        {

        }

        void PrepareDescriptorPool(RenderSystem renderSys)
        {

        }

        void PrepareDescriptorSet(RenderSystem renderSys)
        {

        }

        void PrepareFrameBuffer(RenderSystem renderSys)
        {

        }

        void RenderFrame(RenderSystem renderSys)
        {

        }

        void RenderWindowClosed()
        {
            run = false;
        }
    }
}
