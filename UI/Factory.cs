using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;
namespace LiveSplit.Cuphead {
	public class Factory : IComponentFactory {
		public string ComponentName { get { return "Cuphead Autosplitter v" + this.Version.ToString(); } }
		public string Description { get { return "Cuphead Autosplitter"; } }
		public ComponentCategory Category { get { return ComponentCategory.Control; } }
		public IComponent Create(LiveSplitState state) { return new Component(state); }
		public string UpdateName { get { return this.ComponentName; } }
		public string UpdateURL { get { return "https://raw.githubusercontent.com/ShootMe/LiveSplit.Cuphead/master/"; } }
		public string XMLURL { get { return this.UpdateURL + "Components/Updates.xml"; } }
		public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
	}
}