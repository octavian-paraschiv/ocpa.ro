using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki.CustomRenderers;

public abstract class CustomRendererBase
{
    const string delim = "```";

    public abstract string BlockType { get; }

    public virtual async Task<string> RenderBody(string body, Action<Exception> exceptionHandler = null)
    {
        try
        {
            int idx = 0, si = 0, ei = 0;

            string start = delim + BlockType;

            List<string> blocks = new List<string>();

            do
            {
                si = body.IndexOf(start, idx);
                if (si > 0)
                {
                    blocks.Add(body.Substring(idx, si - idx));
                    ei = body.IndexOf(delim, si + start.Length);
                    if (ei > 0)
                    {
                        var rawBlock = body.Substring(si + start.Length, ei - si - start.Length - 1);
                        var renderedBlock = await RenderBlock(rawBlock);

                        blocks.Add(renderedBlock);

                        idx = ei + delim.Length;
                    }
                }
                else
                    break;
            }
            while (idx < body.Length);

            if (blocks.Count > 0)
                return string.Join("", blocks);
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
        }

        return body;
    }

    protected abstract Task<string> RenderBlock(string block);
}
