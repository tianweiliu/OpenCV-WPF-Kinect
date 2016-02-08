using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace raw_streams.cs
{
    public class PXCMDevice
    {
        public PXCMCapture.DeviceInfo device;
        public int iuid;
        public PXCMDevice(PXCMCapture.DeviceInfo device, int iuid)
        {
            this.device = device;
            this.iuid = iuid;
        }
    }

    class RenderStreams
    {
        public RenderStreams()
        {
        }

        public static int ALIGN16(uint width)
        {
            return ((int)((width + 15) / 16)) * 16;
        }

        public static byte[] GetRGB32Pixels(PXCMImage image)
        {
            int cwidth = ALIGN16(image.info.width); /* aligned width */
            int cheight = (int)image.info.height;

            PXCMImage.ImageData cdata;
            byte[] cpixels;
            if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out cdata)>=pxcmStatus.PXCM_STATUS_NO_ERROR) 
            {
                cpixels = cdata.ToByteArray(0, cwidth * cheight * 4);
                image.ReleaseAccess(ref cdata);
            }
            else
            {
                cpixels = new byte[cwidth * cheight * 4];
            }
            return cpixels;
        }

        public void RunColorDepthAsync(PXCMDevice device, PXCMCapture.VideoStream.ProfileInfo cinfo, PXCMCapture.VideoStream.ProfileInfo dinfo, KinectWPFOpenCV.MainWindow form) /* Stream color and depth independently */
        {
            bool sts = true;

            PXCMSession session;
            pxcmStatus sts2 = PXCMSession.CreateInstance(out session);
            if (sts2 < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                form.sensor_StatusChanged(this, "Failed to create an SDK session");
                return;
            }

            /* UtilMCapture works best for asychronous color and depth streaming */
            UtilMCapture uc = new UtilMCapture(session);

            /* Set Inpt Source */
            PXCMCapture.DeviceInfo dinfo2 = device.device;
            uc.SetFilter(ref dinfo2);

            /* Set Color & Depth Resolution */
            int nstreams = 0;
            PXCMCapture.VideoStream.DataDesc desc = new PXCMCapture.VideoStream.DataDesc();
            PXCMCapture.VideoStream.DataDesc.StreamDesc sdesc = new PXCMCapture.VideoStream.DataDesc.StreamDesc();

            //PXCMCapture.VideoStream.ProfileInfo cinfo = form.GetColorConfiguration();
            if (cinfo.imageInfo.format!=0)
            {
                uc.SetFilter(ref cinfo); // only needed to set FPS
                sdesc.format = cinfo.imageInfo.format;
                sdesc.sizeMin.width = cinfo.imageInfo.width;
                sdesc.sizeMin.height = cinfo.imageInfo.height;
                desc.streams[nstreams++] = sdesc;
            }

            //PXCMCapture.VideoStream.ProfileInfo dinfo = form.GetDepthConfiguration();
            if (dinfo.imageInfo.format!=0)
            {
                uc.SetFilter(ref dinfo); // only needed to set FPS
                sdesc.format = dinfo.imageInfo.format;
                sdesc.sizeMin.width = dinfo.imageInfo.width;
                sdesc.sizeMin.height = dinfo.imageInfo.height;
                desc.streams[nstreams++] = sdesc;
            }

            /* Initialization */
            form.sensor_StatusChanged(this, "Init Started");
            if (uc.LocateStreams(ref desc)>=pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                PXCMImage[] images = new PXCMImage[nstreams];
                PXCMScheduler.SyncPoint[] sps = new PXCMScheduler.SyncPoint[nstreams];
                int[] panels = new int[2] { 0, 1 };

                /* initialize first read */
                for (int i = 0; i < nstreams; i++)
                    sts2=uc.QueryVideoStream(i).ReadStreamAsync(out images[i], out sps[i]);

                form.sensor_StatusChanged(this, "Streaming");
                //FPSTimer timer=new FPSTimer(form);
                while (!form.GetStopState())
                {
                    uint idx;
                    PXCMScheduler.SyncPoint.SynchronizeEx(sps, out idx); /* wait until a sample is ready */

                    /* If raw depth is needed, disable smoothing */
                    uc.device.SetProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_SMOOTHING, form.GetDepthRawState() ? 0 : 1);

                    /* Set main panel and PIP panel index */
                    panels[0] = ((form.GetDepthState() || form.GetDepthRawState()) && nstreams>1)?1:0;
                    panels[1] = 1 - panels[0];

                    for (int i = (int)idx; i < nstreams; i++) /* loop through all streams for all available streams */
                    {
                        sts2 = sps[i].Synchronize(0);
                        if (sts2 == pxcmStatus.PXCM_STATUS_EXEC_INPROGRESS) continue;
                        if (sts2 < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                        /* initiate next read */
                        PXCMImage picture=images[i];
                        images[i] = null; sps[i].Dispose();
                        sts2 = uc.QueryVideoStream(i).ReadStreamAsync(out images[i], out sps[i]);

                        /* Display only the selected picture */
                        form.SetBitmap(panels[i], ALIGN16(desc.streams[i].sizeMin.width), (int)desc.streams[i].sizeMin.height, GetRGB32Pixels(picture));
                        //if (panels[i]==0) timer.Tick(picture.info.format.ToString().Substring(13) + " " + desc.streams[i].sizeMin.width + "x" + desc.streams[i].sizeMin.height);
                        picture.Dispose();

                        if (sts2 < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    }
                    if (sts2 == pxcmStatus.PXCM_STATUS_EXEC_INPROGRESS) continue;
                    if (sts2 < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    //form.UpdatePanel();
                }
                PXCMScheduler.SyncPoint.SynchronizeEx(sps);
                PXCMImage.Dispose(images);
                PXCMScheduler.SyncPoint.Dispose(sps);
            }
            else
            {
                form.sensor_StatusChanged(this, "Init Failed");
                sts = false;
            }

            uc.Dispose();
            session.Dispose();
            if (sts) form.sensor_StatusChanged(this, "Stopped");
        }
    }
}
