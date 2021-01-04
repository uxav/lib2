using System;
using Crestron.SimplSharp.Reflection;

namespace UX.Lib2.Config
{
    public class AConfig : IConfig
    {
        #region Fields
        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public AConfig()
        {
            var type = GetType().GetCType();

            foreach (var field in type.GetFields())
            {
                if (field.FieldType == typeof(String))
                {
                    if (field.Name == "Name")
                        field.SetValue(this, string.Format("{0} Name", type.Name));
                    else if (field.Name.Contains("Name"))
                        field.SetValue(this, string.Format("{0} {1}", type.Name, field.Name));
                    else
                        field.SetValue(this, "");
                }
                else if (field.FieldType == typeof(String))
                    field.SetValue(this, "");
                else if (field.FieldType == typeof(Int32))
                    field.SetValue(this, 0);
                else if (field.FieldType == typeof(UInt32))
                    field.SetValue(this, (uint)0);
                else if (field.FieldType == typeof(Boolean))
                    field.SetValue(this, false);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public uint Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }

        #endregion

        #region Methods
        #endregion
    }
}