using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

const string exe = @"C:\Users\Cunza\Downloads\vector (5).exe";
const string outDir = @"c:\Users\Cunza\Desktop\vectorstrap if it was made correctly and actually in a decent way and manner\Bloxstrap";

var sizes = new[] { 256, 128, 96, 64, 48, 32, 24, 16 };
var pngSizes = new List<(int Size, Bitmap Bmp)>();

foreach (var size in sizes)
{
    if (!TryExtractIcon(exe, 0, size, out var icon))
        continue;

    using (icon)
    {
        Console.WriteLine($"PrivateExtractIcons {size}: {icon.Width}x{icon.Height}");
        using var bmp = IconToBitmapArgb(icon);
        pngSizes.Add((size, (Bitmap)bmp.Clone()));
    }
}

if (pngSizes.Count == 0)
{
    Console.WriteLine("No icons extracted from index 0; trying index 1...");
    foreach (var size in sizes)
    {
        if (!TryExtractIcon(exe, 1, size, out var icon))
            continue;

        using (icon)
        {
            Console.WriteLine($"PrivateExtractIcons idx=1 {size}: {icon.Width}x{icon.Height}");
            using var bmp = IconToBitmapArgb(icon);
            pngSizes.Add((size, (Bitmap)bmp.Clone()));
        }
    }
}

if (pngSizes.Count == 0)
{
    Console.WriteLine("No icons extracted.");
    return 1;
}

pngSizes.Sort((a, b) => b.Size.CompareTo(a.Size));
var largest = pngSizes[0].Bmp;
Console.WriteLine($"Using {largest.Width}x{largest.Height} as master; writing multi-size ICO ({pngSizes.Count} sizes)");

WriteMultiSizeIco(pngSizes.Select(p => p.Bmp).ToList(), Path.Combine(outDir, "Vectorstrap.ico"));

var logo256 = pngSizes.First(p => p.Size == 256).Bmp;
logo256.Save(Path.Combine(outDir, "Resources", "VectorstrapLogo.png"), ImageFormat.Png);

var logo32 = pngSizes.FirstOrDefault(p => p.Size == 32).Bmp ?? ResizeBitmap(largest, 32, 32);
logo32.Save(Path.Combine(outDir, "Resources", "VectorstrapLogo32.png"), ImageFormat.Png);

foreach (var (_, bmp) in pngSizes)
    bmp.Dispose();

return 0;

static bool TryExtractIcon(string path, int index, int size, out Icon? icon)
{
    icon = null;
    var icons = new IntPtr[1];
    var ids = new IntPtr[1];
    uint count = PrivateExtractIcons(path, index, size, size, icons, ids, 1, 0);
    if (count == 0 || icons[0] == IntPtr.Zero)
        return false;

    icon = (Icon)Icon.FromHandle(icons[0]).Clone();
    DestroyIcon(icons[0]);
    return true;
}

static Bitmap IconToBitmapArgb(Icon icon)
{
    var bmp = new Bitmap(icon.Width, icon.Height, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(bmp))
    {
        g.Clear(Color.Transparent);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.DrawIcon(icon, new Rectangle(0, 0, icon.Width, icon.Height));
    }

    return bmp;
}

static Bitmap ResizeBitmap(Bitmap source, int width, int height)
{
    var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(bmp))
    {
        g.Clear(Color.Transparent);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
        g.DrawImage(source, new Rectangle(0, 0, width, height));
    }

    return bmp;
}

static void WriteMultiSizeIco(IReadOnlyList<Bitmap> images, string path)
{
    using var fs = File.Create(path);
    using var bw = new BinaryWriter(fs);

    bw.Write((ushort)0);
    bw.Write((ushort)1);
    bw.Write((ushort)images.Count);

    var offset = 6 + 16 * images.Count;
    var pngData = new List<byte[]>();

    foreach (var img in images)
    {
        using var ms = new MemoryStream();
        img.Save(ms, ImageFormat.Png);
        var data = ms.ToArray();
        pngData.Add(data);

        var w = img.Width >= 256 ? (byte)0 : (byte)img.Width;
        var h = img.Height >= 256 ? (byte)0 : (byte)img.Height;

        bw.Write(w);
        bw.Write(h);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((ushort)1);
        bw.Write((ushort)32);
        bw.Write((uint)data.Length);
        bw.Write((uint)offset);
        offset += data.Length;
    }

    foreach (var data in pngData)
        bw.Write(data);
}

[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "PrivateExtractIconsW", ExactSpelling = true)]
static extern uint PrivateExtractIcons(
    string szFileName,
    int nIconIndex,
    int cxIcon,
    int cyIcon,
    IntPtr[] phicon,
    IntPtr[] piconid,
    uint nIcons,
    uint flags);

[DllImport("user32.dll", SetLastError = true)]
static extern bool DestroyIcon(nint hIcon);
