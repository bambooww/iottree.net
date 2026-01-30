using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iottree.lib
{
    public class IOTTreeTag
    {
        string prj_name;

        string path; //full path

        int iid;

        string title;

        string tp; //int32 bool etc

        public IOTTreeTag(string prj_name, string path, int iid, string title, string tp)
        {
            this.prj_name = prj_name;
            this.path = path;
            this.iid = iid;
            this.title = title;
            this.tp = tp;
        }

        public IOTTreeTag(TagItem ti)
        {
            this.prj_name = ti.PrjName;
            this.path = ti.Path;
            this.iid = ti.Iid;
            this.title = ti.Title;
            this.tp = ti.Tp;
        }

        public string PrjName { get { return this.prj_name; } }

        public int IID { get { return this.iid; } }

        public string Path { get { return this.path; } }

        public string Title { get { return this.title; } }

        public string TP { get { return this.tp; } }
    }

    public class IOTTreeTagVal
    {
        IOTTreeTag tag;

        long update_dt;
        long change_dt;
        string str_val;
        bool valid;

        public IOTTreeTagVal(IOTTreeTag tag, long update_dt, long change_dt, string str_val, bool valid)
        {
            this.tag = tag;
            this.update_dt = update_dt;
            this.change_dt = change_dt;
            this.str_val = str_val;
            this.valid = valid;
        }

        public IOTTreeTag Tag { get { return this.tag; } }

        public string Path {get { return this.tag.Path; }}

        public long UpdateDT { get { return this.update_dt; } }

        public DateTime UpdateTime
        {
            get
            {
                if (this.update_dt <= 0) return DateTime.MinValue;
                return DateTimeOffset.FromUnixTimeMilliseconds(this.update_dt).UtcDateTime;
            }
        }

        public long ChangeDT { get { return this.change_dt; } }

        public DateTime ChangeTime
        {
            get
            {
                if (this.change_dt <= 0) return DateTime.MinValue;
                return DateTimeOffset.FromUnixTimeMilliseconds(this.change_dt).UtcDateTime;

            }
        }

        public bool Valid { get { return this.valid; } }

        public string StrVal { get { return this.str_val; }}

        internal void setUpdateVal(long up_dt,long chg_dt,bool valid,string strval)
        {
            this.update_dt = up_dt;
            this.change_dt = chg_dt;
            this.str_val = strval;
            this.valid = valid;
        }

        public object ObjVal { get {
                switch(tag.TP)
                {
                    case "none":
                        return null;
                    case "bool":
                        return "true".Equals(str_val) || "1".Equals(str_val);
                    case "byte":
                        return Byte.Parse(this.str_val);
                    case "char":
                        return Char.Parse(this.str_val);
                    case "int16":
                    case "uint8":
                        return short.Parse(this.str_val);
                    case "int32":
                        return int.Parse(this.str_val);
                    case "int64":
                        return long.Parse(this.str_val);
                    case "uint16":
                        return UInt16.Parse(this.str_val);
                    case "uint32":
                        return UInt32.Parse(this.str_val);
                    case "uint64":
                        return UInt64.Parse(this.str_val);
                    case "float":
                        return float.Parse(this.str_val);
                    case "double":
                        return double.Parse(this.str_val);
                    case "str":
                    case "date":
                    default:
                        return this.str_val;
                }
            } }

        public bool getValBool(bool def)
        {
            switch (tag.TP)
            {
                case "bool":
                    return "true".Equals(str_val) || "1".Equals(str_val);
            }
            return def;
        }

        public short getValShort(short def)
        {
            switch (tag.TP)
            {

                case "byte":
                    return Byte.Parse(this.str_val);
                case "char":
                    return (short)Char.Parse(this.str_val);
                case "int16":
                case "uint8":
                    return short.Parse(this.str_val);
                case "int32":
                    return (short)int.Parse(this.str_val);
                case "int64":
                    return (short)long.Parse(this.str_val);
                case "uint16":
                    return (short)UInt16.Parse(this.str_val);
                case "uint32":
                    return (short)UInt32.Parse(this.str_val);
                case "uint64":
                    return (short)UInt64.Parse(this.str_val);
            }
            return def;
        }

        public short getValInt16(short def)
        {
            switch (tag.TP)
            {

                case "byte":
                    return Byte.Parse(this.str_val);
                case "char":
                    return (short)Char.Parse(this.str_val);
                case "int16":
                case "uint8":
                    return short.Parse(this.str_val);
                case "int32":
                    return (short)int.Parse(this.str_val);
                case "int64":
                    return (short)long.Parse(this.str_val);
                case "uint16":
                    return (short)UInt16.Parse(this.str_val);
                case "uint32":
                    return (short)UInt32.Parse(this.str_val);
                case "uint64":
                    return (short)UInt64.Parse(this.str_val);
            }
            return def;
        }

        public int getValInt32(int def)
        {
            switch (tag.TP)
            {

                case "byte":
                    return Byte.Parse(this.str_val);
                case "char":
                    return (short)Char.Parse(this.str_val);
                case "int16":
                case "uint8":
                    return short.Parse(this.str_val);
                case "int32":
                    return int.Parse(this.str_val);
                case "int64":
                    return (int)long.Parse(this.str_val);
                case "uint16":
                    return (int)UInt16.Parse(this.str_val);
                case "uint32":
                    return (int)UInt32.Parse(this.str_val);
                case "uint64":
                    return (int)UInt64.Parse(this.str_val);
            }
            return def;
        }

        public long getValInt64(long def)
        {
            switch (tag.TP)
            {

                case "byte":
                    return Byte.Parse(this.str_val);
                case "char":
                    return (short)Char.Parse(this.str_val);
                case "int16":
                case "uint8":
                    return short.Parse(this.str_val);
                case "int32":
                    return int.Parse(this.str_val);
                case "int64":
                    return long.Parse(this.str_val);
                case "uint16":
                    return (long)UInt16.Parse(this.str_val);
                case "uint32":
                    return (long)UInt32.Parse(this.str_val);
                case "uint64":
                    return (long)UInt64.Parse(this.str_val);
            }
            return def;
        }

        public float getValFloat(float def)
        {
            switch (tag.TP)
            {
                case "float":
                    return float.Parse(this.str_val);
                case "double":
                    return (float)double.Parse(this.str_val);
            }
            return def;
        }

        public double getValDouble(double def)
        {
            switch (tag.TP)
            {
                case "float":
                    return float.Parse(this.str_val);
                case "double":
                    return double.Parse(this.str_val);
            }
            return def;
        }

        public override string ToString()
        {
            return $"{this.Path} {this.valid} {this.UpdateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} {ChangeTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} = {StrVal}"; 

        }
    }
}
