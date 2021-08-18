using System;

namespace fi.Core
{
    /// <summary>
    /// Enum Member degerleri
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EnumMemberInfoAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; }

        private Guid _guid;

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public Guid Guid
        {
            get
            {
                if (_guid == Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(_guidString))
                    {
                        _guid = Guid.Parse(_guidString);
                    }
                }

                return _guid;
            }
        }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; }

        private Guid? _parentGuid;

        /// <summary>
        /// Gets the parent GUID.
        /// </summary>
        /// <value>
        /// The parent GUID.
        /// </value>
        public Guid? ParentGuid
        {
            get
            {
                if (!_parentGuid.HasValue)
                {
                    if (!string.IsNullOrEmpty(_parentGuidString))
                    {
                        _parentGuid = Guid.Parse(_parentGuidString);
                    }
                }

                return _parentGuid;
            }
        }

        private readonly string _guidString;
        private readonly string _parentGuidString;

        public int SortOrder { get; set; }

        public bool IsDefault { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMemberInfoAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public EnumMemberInfoAttribute(string name, string description, string guid)
        {
            Name = name;
            Description = description;
            _guidString = guid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMemberInfoAttribute"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public EnumMemberInfoAttribute(string name, string description, string guid, string code)
            : this(name, description, guid)
        {
            Code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMemberInfoAttribute"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="parentGuid">The parent GUID.</param>
        public EnumMemberInfoAttribute(string name, string description, string guid, string code, string parentGuid)
            : this(name, description, guid, code)
        {
            _parentGuidString = parentGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMemberInfoAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public EnumMemberInfoAttribute(string name, string description, string guid, string code, string parentGuid, int sortOrder, bool IsDefault = false)
            : this(name, description, guid, code, parentGuid)
        {
            SortOrder = sortOrder;
            this.IsDefault = IsDefault;
        }
    }
}
