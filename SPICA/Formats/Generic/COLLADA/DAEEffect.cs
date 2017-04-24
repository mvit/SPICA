﻿using SPICA.Formats.CtrH3D.Model.Material;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SPICA.Formats.Generic.COLLADA
{
    public class DAEEffect
    {
        [XmlAttribute] public string id;
        [XmlAttribute] public string name;

        public DAEProfileCOMMON profile_COMMON = new DAEProfileCOMMON();
    }

    public class DAEProfileCOMMON
    {
        [XmlAttribute] public string sid;

        [XmlElement("newparam")] public List<DAEEffectParam> newparam = new List<DAEEffectParam>();

        public DAEEffectProfileCOMMONTechnique technique = new DAEEffectProfileCOMMONTechnique();
    }

    public class DAEEffectParam
    {
        [XmlAttribute] public string sid;

        [XmlElement(IsNullable = false)] public DAEEffectParamSurfaceElement surface;
        [XmlElement(IsNullable = false)] public DAEEffectParamSampler2DElement sampler2D;
    }

    public class DAEEffectParamSurfaceElement
    {
        [XmlAttribute] public string type;

        public string init_from;
        public string format;
    }

    public class DAEEffectParamSampler2DElement
    {
        public string source;
        public DAEWrap wrap_s;
        public DAEWrap wrap_t;
        public DAEFilter minfilter;
        public DAEFilter magfilter;
        public DAEFilter mipfilter;
    }

    public enum DAEWrap
    {
        NONE,
        WRAP,
        MIRROR,
        CLAMP,
        BORDER
    }

    public enum DAEFilter
    {
        NONE,
        NEAREST,
        LINEAR,
        NEAREST_MIPMAP_NEAREST,
        LINEAR_MIPMAP_NEAREST,
        NEAREST_MIPMAP_LINEAR,
        LINEAR_MIPMAP_LINEAR
    }

    public static class DAEH3DTextureWrapExtensions
    {
        public static DAEWrap ToDAEWrap(this H3DTextureWrap Wrap)
        {
            switch (Wrap)
            {
                case H3DTextureWrap.ClampToEdge:   return DAEWrap.CLAMP;
                case H3DTextureWrap.ClampToBorder: return DAEWrap.BORDER;
                case H3DTextureWrap.Repeat:        return DAEWrap.WRAP;
                case H3DTextureWrap.Mirror:        return DAEWrap.MIRROR;

                default: throw new ArgumentException("Invalid Texture wrap!");
            }
        }
    }

    public static class DAEH3DTextureFilterExtensions
    {
        public static DAEFilter ToDAEFilter(this H3DTextureMinFilter Filter)
        {
            switch (Filter)
            {
                case H3DTextureMinFilter.Nearest:              return DAEFilter.NEAREST;
                case H3DTextureMinFilter.NearestMipmapNearest: return DAEFilter.NEAREST_MIPMAP_NEAREST;
                case H3DTextureMinFilter.NearestMipmapLinear:  return DAEFilter.NEAREST_MIPMAP_LINEAR;
                case H3DTextureMinFilter.Linear:               return DAEFilter.LINEAR;
                case H3DTextureMinFilter.LinearMipmapNearest:  return DAEFilter.LINEAR_MIPMAP_NEAREST;
                case H3DTextureMinFilter.LinearMipmapLinear:   return DAEFilter.LINEAR_MIPMAP_LINEAR;

                default: throw new ArgumentException("Invalid Minification filter!");
            }
        }

        public static DAEFilter ToDAEFilter(this H3DTextureMagFilter Filter)
        {
            switch (Filter)
            {
                case H3DTextureMagFilter.Nearest: return DAEFilter.NEAREST;
                case H3DTextureMagFilter.Linear:  return DAEFilter.LINEAR;

                default: throw new ArgumentException("Invalid Magnification filter!");
            }
        }
    }

    public class DAEEffectProfileCOMMONTechnique
    {
        [XmlAttribute] public string sid;

        public DAEPhong phong = new DAEPhong();
    }

    public class DAEPhong
    {
        public DAEPhongDiffuse diffuse = new DAEPhongDiffuse();
    }

    public class DAEPhongDiffuse
    {
        public DAEPhongDiffuseTexture texture = new DAEPhongDiffuseTexture();
    }

    public class DAEPhongDiffuseTexture
    {
        [XmlAttribute] public string texture;
        [XmlAttribute] public string texcoord = "uv";
    }
}
