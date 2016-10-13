using mRemoteNG.Tools;
using System.ComponentModel;
using mRemoteNG.Container;


namespace mRemoteNG.Tree.Root
{
	[DefaultProperty("Name")]
    public class RootNodeInfo : ContainerInfo
	{
	    private string _name;

	    public RootNodeInfo(RootNodeType rootType)
		{
            _name = Language.strConnections;
			Type = rootType;
		}
		
        #region Public Properties

	    [LocalizedAttributes.LocalizedCategory("strCategoryDisplay"),
	     Browsable(true),
	     LocalizedAttributes.LocalizedDefaultValue("strConnections"),
	     LocalizedAttributes.LocalizedDisplayName("strPropertyNameName"),
	     LocalizedAttributes.LocalizedDescription("strPropertyDescriptionName")]
	    public override string Name
	    {
	        get { return _name; }
            set { _name = value; }
	    }

	    [LocalizedAttributes.LocalizedCategory("strCategoryDisplay"),
            Browsable(true),
            LocalizedAttributes.LocalizedDisplayName("strPasswordProtect"),
            TypeConverter(typeof(Tools.MiscTools.YesNoTypeConverter))]
        public new bool Password { get; set; }
			
		[Browsable(false)]
        public string PasswordString {get; set;}
			
		[Browsable(false)]
        public RootNodeType Type {get; set;}

        public override TreeNodeType GetTreeNodeType()
        {
            return TreeNodeType.Root;
        }
        #endregion
    }
}