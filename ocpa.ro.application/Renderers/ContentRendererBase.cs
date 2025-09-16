using System;
using System.Collections.Generic;

namespace ocpa.ro.application.Renderers;

public abstract class ContentRendererBase
{
    const string delim = "```";

    public abstract string BlockType { get; }

    public virtual string RenderBody(string body, Action<Exception> exceptionHandler = null)
    {
        try
        {
            int currentPos = 0, si = 0, ei = 0;

            string start = delim + BlockType;

            List<string> blocks = new List<string>();

            do
            {
                si = body.IndexOf(start, currentPos);
                if (si > 0)
                {
                    blocks.Add(body.Substring(currentPos, si - currentPos));
                    ei = body.IndexOf(delim, si + start.Length);
                    if (ei > 0)
                    {
                        var rawBlock = body.Substring(si + start.Length, ei - si - start.Length - 1);
                        string renderedBlock = null;

                        try
                        {
                            renderedBlock = RenderBlock(rawBlock);
                        }
                        catch (Exception ex)
                        {
                            exceptionHandler?.Invoke(ex);
                            renderedBlock = null;
                        }

                        if (renderedBlock?.Length > 0)
                            blocks.Add(renderedBlock ?? rawBlock);
                        else
                            blocks.Add(rawBlock);

                        currentPos = ei + delim.Length;
                    }
                }
                else
                    break;
            }
            while (currentPos < body.Length);

            if (currentPos < body.Length)
                blocks.Add(body.Substring(currentPos, body.Length - currentPos));

            if (blocks.Count > 0)
                return string.Join("", blocks);
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
        }

        return body;
    }

    protected abstract string RenderBlock(string block);
}
