using System.Collections.Generic;

namespace Utilizr.FileSystem
{
    public static class FileExtensions
    {
        public static List<string> Videos = new()
        {
            "0.264", "3g2", "3gp", "3gp2", "3gpp", "3gpp2", "3mm", "3p2", "60d", "aaf", "aec", "aep", "aepx", "aet", "aetx", "ajp",
            "ale", "am", "amc", "amv", "amx", "anim", "aqt", "arcut", "arf", "asf", "asx", "avb", "avc", "avd", "avi", "avp", "avs",
            "avs", "avv", "axm", "bdm", "bdmv", "bdt2", "bdt3", "bik", "bin", "bix", "bmk", "bnp", "box", "bs4", "bsf", "bvr", "byu",
            "camproj", "camrec", "camv", "ced", "cel", "cine", "cip", "clpi", "cmmp", "cmmtpl", "cmproj", "cmrec", "cpi", "cst", "cvc",
            "cx3", "d2v", "d3v", "dce", "divx", "dlx", "dmb", "dmsd", "dmsd3d", "dmsm", "dmsm3d", "dmss", "dmx", "dpg", "dsy", "dv",
            "dv-avi", "dv4", "dvdmedia", "dvr", "dvr-ms", "dvx", "dxr", "dzm", "dzp", "dzt", "edl", "evo", "eye", "ezt", "f4p", "f4v",
            "fbr", "fbr", "fbz", "fcp", "fcproject", "flc", "gfp", "gl", "gom", "grasp", "gts", "gvi", "gvp", "h264", "hdmov", "hkm",
            "ifo", "imovieproj", "imovieproject", "ircp", "irf", "ism", "ismc", "ismv", "iva", "ivf", "ivr", "ivs", "izz", "izzy", "jss",
            "jts", "jtv", "k3g", "lrec", "lsf", "lsx", "m15", "m1pg", "m1v", "m21", "m21", "m2a", "m2p", "m2t", "m2ts", "m2v", "m4e",
            "m4u", "m4v", "m75", "mani", "meta", "mgv", "mj2", "mjp", "mjpg", "mk3d", "mkv", "mmv", "mnv", "mob", "mod", "modd", "moff",
            "moi", "moov", "mov", "movie", "mp21", "mp21", "mp2v", "mp4", "mp4v", "mpe", "mpeg", "mpeg4", "mpf", "mpg", "mpg2", "mpgindex",
            "mpl", "mpl", "mpls", "mpsub", "mpv", "mpv2", "mqv", "msdvd", "mse", "msh", "mswmm", "mts", "mtv", "mvb", "mvc", "mvd", "mve",
            "mvex", "mvp", "mvp", "mvy", "mxf", "mys", "ncor", "nsv", "nuv", "nvc", "ogm", "ogv", "ogx", "osp", "otrkey", "pac", "par",
            "pds", "pgi", "photoshow", "piv", "pjs", "plproj", "pmv", "pvr", "pxv", "qt", "qtch", "qtindex", "qtl", "qtm", "qtz", "r3d",
            "rcd", "rcproject", "rdb", "rec", "rm", "rmd", "rmd", "rmp", "rms", "rmv", "rmvb", "roq", "rp", "rsx", "rts", "rts", "rum",
            "rv", "rvl", "sbk", "sbt", "scc", "scm", "scm", "scn", "screenflow", "sedprj", "sfd", "sfvidcap", "svi", "tda3mt", "tdx",
            "thp", "tivo", "ts", "tsp", "ttxt", "tvs", "usf", "usm", "vc1", "vcpf", "vcr", "vcv", "vdo", "vdr", "vdx", "veg", "vem", "vep",
            "vf", "vft", "vfw", "vfz", "vgz", "vid", "video", "viewlet", "viv", "vivo", "vlab", "vob", "vp3", "vp6", "vp7", "vpj", "vro",
            "vs4", "vse", "vsp", "w32", "wcp", "webm", "wlmp", "wm", "wmd", "wmmp", "wmv", "wmx", "wot", "wp3", "wpl", "wtv", "wvx",
            "xej", "xel", "xesc", "xfl", "xlmv", "xvid", "yuv", "zeg", "zm1", "zm2", "zm3", "zmv"
        };

        public static List<string> Music = new()
        {
            "2sf,", "aac", "aiff", "amr", "ape", "asf", "ast", "au", "aup", "band", "brstm", "bwf", "cdda", "cust", "dsf", "dwd", "flac", "gsf",
            "gsm", "gym", "iff-16sv", "iff-8svx", "it", "jam", "la", "ly", "m4a", "m4p", "mid", "midi", "minipsf,", "mng", "mod", "mp1", "mp2",
            "mp3", "mpc", "mscz", "mscz,", "mt2", "mus", "mxl", "niff", "nsf", "optimfrog", "ots", "pac", "psf", "psf2", "psflib", "ptb", "qsf",
            "ra", "raw", "rka", "rm", "rmj", "s3m", "shn", "sib", "smp", "spc", "spx", "ssf", "swa", "tta", "txm", "usf", "vgm", "voc", "vox",
            "vqf", "wav", "wma", "wv", "xm", "ym"
        };

        public static List<string> Documents = new()
        {
            "602", "abw", "acl", "afp", "amigaguide", "ans", "asc", "aww", "azw", "ccf", "csv", "cwk", "dat", "doc", "docx", "dot", "dotx", "egt",
            "epub", "fdx", "fm", "ftm", "ftx", "html", "hwp", "hwpml", "info", "indd", "key", "log", "lwp", "mbp", "mcw", "mobi", "msg", "nb", "nbp",
            "odm", "odt", "omm", "ott", "pages", "pap", "pdax", "pdf", "pps", "ppt", "pptx", "quox", "rpt", "rtf", "sdw", "stw", "sxw", "tex", "text",
            "troff", "txt", "uof", "uoml", "via", "wpd", "wps", "wpt", "wrd", "wrf", "wri", "xhtml", "xml", "xps", "xls", "xlsx", "xlsx", "xlr"
        };

        public static List<string> Photos = new()
        {
            "2bp", "2bpp", "3fr", "bsv", "aab", "aam", "aas", "adrg", "adri", "afp", "agp", "ai", "alpha", "als", "ami", "ani", "anim", "apx", "art",
            "arw", "awd", "bay", "bef", "bie", "bitmap", "blend", "bm", "bmf", "bmp", "brk", "btf", "bw", "ca1", "ca2", "ca3", "cadrg", "cal", "cals",
            "cam", "cap", "ccx", "cdr", "cdt", "cel", "cgm", "ciff", "cin", "cmx", "cmyk", "cmyka", "cpc", "cpi", "cpt", "cr2", "crw", "cur", "cut",
            "dcl", "dcm", "dcr", "dcs", "dcx", "dds", "dia", "dib", "dicom", "dir", "djv", "djvu", "dng", "doo", "dpx", "drf", "dxf", "dxr", "ecw",
            "edg", "emf", "emz", "epdf", "epi", "eps", "eps2", "eps3", "epsf", "epsi", "ept", "erf", "eri", "eva", "exif", "exr", "fff", "fgd", "fh",
            "fh4", "fh5", "fh7", "fh8", "fhc", "fif", "fig", "fit", "fits", "fla", "flc", "flh", "fli", "flt", "flw", "flx", "fpx", "fxo", "fxs",
            "fxt", "g3", "gbr", "ger", "gfa", "gg", "gif", "giff", "gih", "gra", "hdp", "hdr", "hg", "hpc", "ica", "icns", "ico", "icon", "ics", "ids",
            "iff", "igs", "iiq", "ilbm", "im1", "im24", "im32", "im4", "im8", "ima", "img", "int", "inta", "j2c", "j2k", "j6i", "jbg", "jbig", "jfi",
            "jfif", "jif", "jls", "jng", "jp2", "jpc", "jpe", "jpeg", "jpg", "jpx", "jxr", "k25", "kdc", "kif", "kiff", "koa", "kqp", "lbm", "lsp",
            "mac", "mag", "mask", "matte", "mda", "mdc", "mef", "met", "miff", "mki", "mmr", "mng", "mos", "mpc", "mrw", "msa", "msl", "msp", "mvg",
            "myd", "myv", "nef", "neo", "oaz", "odg", "ora", "orf", "p2", "pam", "pat", "pbm", "pc1", "pc2", "pc3", "pcc", "pcd", "pcf", "pcs", "pct",
            "pct1", "pct2", "pcx", "pdb", "pdd", "pdn", "pef", "pgf", "pgm", "pi", "pi1", "pi2", "pi3", "pic", "pict", "pix", "pld", "pm", "pmp",
            "png", "pnm", "pnt", "ppm", "prc", "ps", "ps2", "ps3", "psb", "psd", "psp", "pspimage", "ptx", "pwp", "px", "pxa", "q0", "q4", "qif", "qti",
            "qtif", "r3d", "rad", "raf", "ras", "raw", "rgb", "rgba", "rgfx", "rgx", "rle", "rs", "rw2", "sai", "sct", "sda", "sdd", "sfw", "sgb",
            "sgi", "sgx", "shape", "sk", "skb", "skp", "sml", "spl", "sr2", "srf", "st4", "st5", "st6", "st7", "st8", "st9", "str", "stx", "sun", "svg",
            "svgz", "swf", "sxd", "tga", "tif", "tiff", "tim", "tlg", "tm2", "tn1", "tn2", "tn3", "tn4", "tn5", "tn6", "tny", "tpic", "tub", "u32", "ufo",
            "vox", "wbm", "wbmp", "wbp", "wdp", "wfx", "wmf", "wmz", "wpg", "x11", "x32", "x3f", "xaml", "xar", "xbm", "xcf", "xcfbz2", "xcfgz", "xjt",
            "xjtbz2", "xjtgz", "xpm", "xwd", "xyz", "ycbcr", "ycbcra", "yuv", "zdt"
        };

        public static List<string> Executables = new()
        {
            "dll", "exe", "com", "bat", "bin", "cmd", "cpl", "gadget", "msi", "job", "msp", "mst", "ps1", "sct", "shb", "ws", "wsf", "reg", "rgs",
            "shs", "u3p", "vb", "vbe", "vbs", "vbscript", "paf", "pif", "lnk", "msc", "jse", "inf", "ins", "inx", "scr", "sys"
        };

        public static List<string> Archives = new()
        {
            //TODO: Not an exhaustive archive file extensions list, just has some popular types
            "gz", "bz2", "rar", "zip", "7z"
        };

        private static List<string> WordProcessing = new()
        {
            "doc", "docm", "docx", "dot", "dotx", "gdoc", "odoc", "odt", "ott"
        };

        public static List<string> Spreadsheets = new()
        {
            "csv", "gsheet", "numbers", "ods", "ots", "wks", "xlk", "xls", "xlsb", "xlsm", "xlsx", "xlr", "xlt", "xltm", "xlw"
        };

        public static List<string> Presentations = new()
        {
            "key", "keynote", "odp", "otp", "pps", "ppt", "pptx", "pot", "gslides"
        };

        public static List<string> DiskImages = new()
        {
            "iso", "bin", "dmg", "nrg", "mdf", "uif", "isz", "daa"
        };

        public static List<string> GeneralDataFormatOrMarkup = new()
        {
            "xml", "html", "json", "yaml", "md"
        };
    }
}