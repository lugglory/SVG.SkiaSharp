#if NETFULL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SkiaSharp;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace Svg.Web
{
    /// <summary>
    /// A handler to asynchronously render Scalable Vector Graphics files (usually *.svg or *.xml file extensions).
    /// </summary>
    public class SvgHandler : IHttpAsyncHandler
    {
        Thread t;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // Not used
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            string path = context.Request.PhysicalPath;

            if (!File.Exists(path))
            {
                throw new HttpException(404, "The requested file cannot be found.");
            }

            SvgAsyncRenderState reqState = new SvgAsyncRenderState(context, cb, extraData);
            SvgAsyncRender asyncRender = new SvgAsyncRender(reqState);
            ThreadStart ts = new ThreadStart(asyncRender.RenderSvg);
            t = new Thread(ts);
            t.Start();

            return reqState;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
        }

        protected sealed class SvgAsyncRender
        {
            private SvgAsyncRenderState _state;

            public SvgAsyncRender(SvgAsyncRenderState state)
            {
                this._state = state;
            }

            private void RenderRawSvg()
            {
                this._state._context.Response.ContentType = "image/svg+xml";
                this._state._context.Response.WriteFile(this._state._context.Request.PhysicalPath);
                this._state._context.Response.End();
                this._state.CompleteRequest();
            }

            public void RenderSvg()
            {
                this._state._context.Response.AddFileDependency(this._state._context.Request.PhysicalPath);
                this._state._context.Response.Cache.SetLastModifiedFromFileDependencies();
                this._state._context.Response.Cache.SetETagFromFileDependencies();
                this._state._context.Response.Buffer = false;

                if (this._state._context.Request.Browser.Crawler || !string.IsNullOrEmpty(this._state._context.Request.QueryString["raw"]))
                {
                    this.RenderRawSvg();
                }
                else
                {
                    try
                    {
                        Dictionary<string, string> entities = new Dictionary<string, string>();
                        NameValueCollection queryString = this._state._context.Request.QueryString;

                        for (int i = 0; i < queryString.Count; i++)
                        {
                            entities.Add(queryString.Keys[i], queryString[i]);
                        }

                        SvgDocument document = SvgDocument.Open<SvgDocument>(this._state._context.Request.PhysicalPath, entities);

                        using (var bitmap = document.Draw())
                        {
                            if (bitmap != null)
                            {
                                using (var image = SKImage.FromBitmap(bitmap))
                                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                                {
                                    this._state._context.Response.ContentType = "image/png";
                                    data.SaveTo(this._state._context.Response.OutputStream);
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Trace.TraceError("An error occured while attempting to render the SVG image '" + this._state._context.Request.PhysicalPath + "': " + exc.Message);
                    }
                    finally
                    {
                        this._state._context.Response.End();
                        this._state.CompleteRequest();
                    }
                }
            }
        }

        protected sealed class SvgAsyncRenderState : IAsyncResult
        {
            internal HttpContext _context;
            internal AsyncCallback _callback;
            internal object _extraData;
            private bool _isCompleted = false;
            private ManualResetEvent _callCompleteEvent = null;

            public SvgAsyncRenderState(HttpContext context, AsyncCallback callback, object extraData)
            {
                _context = context;
                _callback = callback;
                _extraData = extraData;
            }

            internal void CompleteRequest()
            {
                _isCompleted = true;
                lock (this)
                {
                    if (this.AsyncWaitHandle != null)
                    {
                        this._callCompleteEvent.Set();
                    }
                }
                if (_callback != null)
                {
                    _callback(this);
                }
            }

            public object AsyncState { get { return (_extraData); } }
            public bool CompletedSynchronously { get { return (false); } }
            public bool IsCompleted { get { return (_isCompleted); } }
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    lock (this)
                    {
                        if (_callCompleteEvent == null)
                        {
                            _callCompleteEvent = new ManualResetEvent(false);
                        }

                        return _callCompleteEvent;
                    }
                }
            }
        }
    }
}
#endif