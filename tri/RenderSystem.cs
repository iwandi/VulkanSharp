using System;
using System.Collections.Generic;

using Vulkan;
using Vulkan.Windows;

// TODO Vulkan : GetSurfacePresentModesKHR cant use Marshal.SizeOf

namespace tri
{
    public class RenderSystem
    {
        const uint VK_MAX_MEMORY_TYPES = 32;

        public event Action WindowClosed;

        readonly uint ApiVersion = Vulkan.Version.Make(1, 0, 0);

        PhysicalDevice[] gpuList;
        QueueInfo[] queueInfoList;
        System.Windows.Forms.Form window;
        PhysicalDeviceMemoryProperties gpuMemoryProperties;

        Dictionary<QueueFlags, CommandPool> pools = new Dictionary<QueueFlags, CommandPool>();
        CommandBuffer cmdSetup;

        public Instance Instance;
        public PhysicalDevice Gpu;
        public Device Device;
        public SurfaceKhr SwapSurface;
        public SwapchainKhr SwapChain;
        public uint BackBufferWidth;
        public uint BackBufferHeight;

        bool Validate<T>(string[] names, T[] list, Func<T,string, bool> check)
        {
            if(list == null)
            {
                return names.Length <= 0;
            }

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
            foreach (PhysicalDevice gpu in gpuList)
            {
                ExtensionProperties[] extensions = gpu.EnumerateDeviceExtensionProperties("");
                if (!ValidateExtentsions(extensionNames, extensions))
                {
                    continue;
                }

                List<DeviceQueueCreateInfo> queueCreateInfo = new List<DeviceQueueCreateInfo>();
                List<QueueInfo> queueInfoList = new List<QueueInfo>();
                QueueFamilyProperties[] queueFamilys = gpu.GetQueueFamilyProperties();
                if (queueFamilys == null || queueFamilys.Length <= 0)
                {
                    continue;
                }
                else
                {
                    for (uint i = 0; i < queueFamilys.Length; i++)
                    {
                        uint queueCount = queueFamilys[i].QueueCount;
                        queueCreateInfo.Add(new DeviceQueueCreateInfo
                        {
                            QueueFamilyIndex = i,
                            QueueCount = queueCount,
                            QueuePriorities = new float[queueCount],
                        });
                        for (uint j = 0; j < queueCount; j++)
                        {
                            queueInfoList.Add(new QueueInfo
                            {
                                queueFamilyIndex = i,
                                queueIndex = j,
                                flags = (QueueFlags)queueFamilys[i].QueueFlags,
                            });
                        }
                    }
                }

                DeviceCreateInfo info = new DeviceCreateInfo()
                {
                    // LayerNames is "deprecated and ignore" in vulkan 1.0 spec
                    QueueCreateInfos = queueCreateInfo.ToArray(),
                    EnabledExtensionNames = extensionNames,
                };

                Device = gpu.CreateDevice(info, null);
                Gpu = gpu;
                this.queueInfoList = queueInfoList.ToArray();

                gpuMemoryProperties = gpu.GetMemoryProperties();

                return true;
            }           

            return false;
        }

        public void CreateWindow(uint width, uint height)
        {
            window = new System.Windows.Forms.Form
            {
                Width = (int)width,
                Height = (int)height,
            };
            window.Closed += (sender,e) =>
            {
                if(WindowClosed != null)
                {
                    WindowClosed();
                }
            };
            window.Show();
        }
        
        CommandPool GetPool(QueueFlags type)
        {
            CommandPool pool;
            if(pools.TryGetValue(type, out pool))
            {
                return pool;
            }
            CommandPoolCreateInfo info = new CommandPoolCreateInfo
            {
                QueueFamilyIndex = GetQueue(type).queueFamilyIndex,
                Flags = (uint)CommandPoolCreateFlags.ResetCommandBuffer,
            };

            pool = Device.CreateCommandPool(info, null);
            pools.Add(type, pool);
            return pool;
        }

        public CommandBuffer CreateCommandBuffer(QueueFlags type)
        {
            return Device.AllocateCommandBuffers(new CommandBufferAllocateInfo
            {
                CommandPool = GetPool(type),
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            });
        }

        public void CreateSwapChain(PresentModeKhr presentMode, uint imageCount)
        {
            SwapchainKhr oldSwapChain = SwapChain;

            if (SwapSurface == null)
            {
                // == Create Surface for Swap Chain
                IntPtr hInstance = System.Runtime.InteropServices.Marshal.GetHINSTANCE(this.GetType().Module);

                Win32SurfaceCreateInfoKhr surfaceInfo = new Win32SurfaceCreateInfoKhr()
                {
                    Hwnd = window.Handle,
                    Flags = 0,
                    Hinstance = hInstance,
                };

                SurfaceKhr surface = Instance.CreateWin32SurfaceKHR(surfaceInfo, null);

                QueueInfo graphicsQueueInfo = GetQueue(QueueFlags.Graphics);
                Queue graphicsQueue = Device.GetQueue(graphicsQueueInfo.queueFamilyIndex, graphicsQueueInfo.queueIndex);

                SwapSurface = surface;
            }

            // == Create Swap Chain

            // TODO : this is bugged in Vulkan-Mono PresentModeKhr can not be called from Marshal.SizeOf
            /*bool presentModeSupported = false;
            PresentModeKhr[] presentModes = Gpu.GetSurfacePresentModesKHR(SwapSurface);
            foreach (PresentModeKhr checkMode in presentModes)
            {
                if(checkMode == presentMode)
                {
                    presentModeSupported = true;
                    break;
                }
            }
            if(!presentModeSupported )
            {
                throw new Exception("PresentMode :" + presentMode + " not supported by gpu.");
            }*/

            SurfaceCapabilitiesKhr surfaceCapabilities = Gpu.GetSurfaceCapabilitiesKHR(SwapSurface);

            if (surfaceCapabilities.CurrentExtent.Width == uint.MaxValue)
            {
                BackBufferWidth = (uint)window.Width;
                BackBufferHeight = (uint)window.Height;
            }
            else
            {
                BackBufferWidth = surfaceCapabilities.CurrentExtent.Width;
                BackBufferHeight = surfaceCapabilities.CurrentExtent.Height;
            }

            uint reqImageCount = surfaceCapabilities.MinImageCount + imageCount;
            if (reqImageCount > 0 && reqImageCount > surfaceCapabilities.MaxImageCount)
            {
                reqImageCount = surfaceCapabilities.MaxImageCount;
            }

            
            SurfaceFormatKhr[] surfaceFormats = Gpu.GetSurfaceFormatsKHR(SwapSurface);
            Format format = surfaceFormats.Length == 1 && surfaceFormats[0].Format == Format.Undefined ?
                Format.B8g8r8a8Unorm : surfaceFormats[0].Format;
            SurfaceTransformFlagsKhr preTransform = (surfaceCapabilities.SupportedTransforms & SurfaceTransformFlagsKhr.Identity) == SurfaceTransformFlagsKhr.Identity ?
                SurfaceTransformFlagsKhr.Identity : surfaceCapabilities.CurrentTransform;

            SwapchainCreateInfoKhr swapChainInfo = new SwapchainCreateInfoKhr
            {
                Surface = SwapSurface,
                MinImageCount = reqImageCount,
                ImageFormat = format,
                ImageColorSpace = surfaceFormats[0].ColorSpace,
                ImageExtent =  new Extent2D
                {
                    Width = BackBufferWidth,
                    Height = BackBufferHeight,
                },
                ImageUsage = (uint)ImageUsageFlags.ColorAttachment,
                PreTransform = preTransform,
                CompositeAlpha = CompositeAlphaFlagsKhr.Opaque,
                ImageArrayLayers = 1,
                ImageSharingMode = SharingMode.Exclusive,
                PresentMode = presentMode,
                // TODO : Vulkan : we cant assing a null swapChain
                //OldSwapchain = oldSwapChain != null ? oldSwapChain : null,
                Clipped = true,
            };
            
            if(oldSwapChain != null)
            {
                // this is a workaround as we cant assing a null one
                swapChainInfo.OldSwapchain = oldSwapChain;

                Device.DestroySwapchainKHR(oldSwapChain, null);
                oldSwapChain = null;
            }

            SwapchainKhr swapChain = Device.CreateSwapchainKHR(swapChainInfo, null);

            // ==  Create Images

            Image[] swapImages = Device.GetSwapchainImagesKHR(swapChain);

            SwapChainBuffer[] buffers = new SwapChainBuffer[swapImages.Length];
            for(uint i = 0; i < buffers.Length; i++)
            {
                ImageViewCreateInfo imageViewInfo = new ImageViewCreateInfo
                {
                    Format = format,
                    Components = new ComponentMapping
                    {
                        R = ComponentSwizzle.R,
                        G = ComponentSwizzle.G,
                        B = ComponentSwizzle.B,
                        A = ComponentSwizzle.A,
                    },
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = (uint)ImageAspectFlags.Color,
                        BaseMipLevel = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1,
                    },
                    ViewType = ImageViewType.View2D,
                    Flags = 0,
                    Image = swapImages[i],
                };

                ImageView view = Device.CreateImageView(imageViewInfo, null);

                buffers[i] = new SwapChainBuffer
                {
                    image = swapImages[i],
                    view = view,
                };
            }
        }

        public void CreateDepth()
        {
            ImageCreateInfo imageInfo = new ImageCreateInfo
            {
                ImageType = ImageType.Image2D,
                Format = Format.D16Unorm,
                Extent = new Extent3D
                {
                    Width = BackBufferWidth,
                    Height = BackBufferHeight,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = (uint)SampleCountFlags.Count1,
                Tiling = ImageTiling.Optimal,
                Usage = (uint)ImageUsageFlags.DepthStencilAttachment,
                Flags = 0,
            };

            Image image = Device.CreateImage(imageInfo, null);
            MemoryRequirements memReq = Device.GetImageMemoryRequirements(image);

            uint memTypeIndex;
            if(!TryGetMemoryTypeFromProperties(memReq.MemoryTypeBits, 0, out memTypeIndex))
            {
                throw new Exception("Failed to create back buffer");
            }

            MemoryAllocateInfo allocInfo = new MemoryAllocateInfo
            {
                AllocationSize = 0,
                MemoryTypeIndex = memTypeIndex,
            };

            DeviceMemory imageMem = Device.AllocateMemory(allocInfo, null);
            Device.BindImageMemory(image, imageMem, 0);

            SetImageLayout(image, ImageAspectFlags.Depth, ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal, 0);

            ImageViewCreateInfo imageViewInfo = new ImageViewCreateInfo
            {
                Image = image,
                Format = imageInfo.Format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = (uint)ImageAspectFlags.Depth,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
                Flags = 0,
                ViewType = ImageViewType.View2D,
            };

            ImageView imageView = Device.CreateImageView(imageViewInfo, null);
        }

        bool TryGetMemoryTypeFromProperties(uint typeBits, uint requirmentsMask, out uint typeIndex)
        {
            for (uint i = 0; i < gpuMemoryProperties.MemoryTypes.Length; i++)
            {
                if((typeBits & 1) == 1)
                {
                    if((gpuMemoryProperties.MemoryTypes[i].PropertyFlags & requirmentsMask) == requirmentsMask)
                    {
                        typeIndex = i;
                        return true;
                    }
                }
                typeBits >>= 1;
            }

            typeIndex = uint.MaxValue;
            return false;
        }

        void SetImageLayout(Image image, ImageAspectFlags aspectMask, ImageLayout oldImageLayout, ImageLayout newImageLayout, AccessFlags srcAccessMask)
        {
            if(cmdSetup == null) 
            {
                cmdSetup = CreateCommandBuffer(QueueFlags.Graphics);
                cmdSetup.Begin(new CommandBufferBeginInfo());
            }

            ImageMemoryBarrier imageMemoryBarrier = new ImageMemoryBarrier
            {
                SrcAccessMask = (uint)srcAccessMask,
                DstAccessMask = 0,
                OldLayout = oldImageLayout,
                NewLayout = newImageLayout,
                Image = image,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = (uint)aspectMask,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
            };

            if(newImageLayout == ImageLayout.TransferDstOptimal)
            {
                imageMemoryBarrier.DstAccessMask = (uint)AccessFlags.TransferRead;
            }
            else if(newImageLayout == ImageLayout.ColorAttachmentOptimal)
            {
                imageMemoryBarrier.DstAccessMask = (uint)AccessFlags.ColorAttachmentWrite;
            }
            else if(newImageLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                imageMemoryBarrier.DstAccessMask = (uint)AccessFlags.DepthStencilAttachmentWrite;
            }
            else if (newImageLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                imageMemoryBarrier.DstAccessMask = (uint)(AccessFlags.ShaderRead | AccessFlags.InputAttachmentRead);
            }

            // TODO Vulkan : we cant set null as this null->m will be called
            //cmdSetup.CmdPipelineBarrier(PipelineStageFlags.TopOfPipe, PipelineStageFlags.TopOfPipe, 0, 0, MemoryBarrier.Null, 0, BufferMemoryBarrier.Null, 1, imageMemoryBarrier);
            cmdSetup.CmdPipelineBarrier(PipelineStageFlags.TopOfPipe, PipelineStageFlags.TopOfPipe, 0, null, null, new ImageMemoryBarrier[] { imageMemoryBarrier });
        }

        QueueInfo GetQueue(QueueFlags flags)
        {
            foreach (QueueInfo info in queueInfoList)
            {
                if((info.flags & flags) == flags)
                {
                    return info;
                }
            }
            throw new Exception("No Queue with flags:" + flags);
        }

        public void Present()
        {

        }

        public void ShutDown()
        {
            Instance.Destroy(null);
        }

        public void HandleEvents()
        {
            System.Windows.Forms.Application.DoEvents();
        }

        class QueueInfo
        {
            public uint queueFamilyIndex;
            public uint queueIndex;
            public QueueFlags flags;
        }

        class SwapChainBuffer
        {
            public Image image;
            public CommandBuffer cmd;
            public ImageView view;
        }
    }
}
