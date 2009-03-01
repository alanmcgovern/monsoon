
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Monsoon
{
	public abstract class XmlSettings <T> : SettingsController <T>
		where T : new ()
	{
		protected U[] Load <U> (string path)
		{
			if(!File.Exists(path))
			   return new U [0];
			
			try {
				using (FileStream fs = File.Open(path, FileMode.Open)) {
					XmlSerializer xs = new XmlSerializer(typeof(U []));				
					return (U []) xs.Deserialize(fs);
				}
			} catch (XmlException) {
				return new U [0];
			} catch (FileNotFoundException) {
				return new U [0];
			} catch (Exception ex) {
				return new U [0];
			}
		}
		
		protected void Save <U> (string path, U [] data)
		{
			using (Stream fs = new FileStream (path, FileMode.Create)) {
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);				
				XmlSerializer s = new XmlSerializer (typeof(U []));
				s.Serialize (writer, data); 	
			}
		}
	}
}
