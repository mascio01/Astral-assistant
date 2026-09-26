using System.Runtime.InteropServices;

public class GeologicalMapEditor : DebugMenu
{
	public GeologicalMapEditor()
		: base(GameImpl.Translate("DEBUG_GeologicalMapEditor"))
	{
		for (int i = 0; i < 4; i++)
		{
			MineralType mineralType = (MineralType)i;
			Items.Add(new DebugMenuItemCustom("Load " + mineralType.ToString() + " png", delegate
			{
				OpenFileName openFileName = new OpenFileName();
				openFileName.structSize = Marshal.SizeOf(openFileName);
				openFileName.filter = "png files (*.png)\0*.png\0All files (*.*)\0*.*\0\0";
				openFileName.file = new string(new char[256]);
				openFileName.maxFile = openFileName.file.Length;
				openFileName.fileTitle = new string(new char[64]);
				openFileName.maxFileTitle = openFileName.fileTitle.Length;
				openFileName.initialDir = GameImpl.Instance.SaveGamePath;
				openFileName.title = "Open Geological Map as PNG Image";
				openFileName.defExt = "PNG";
				openFileName.flags = 530440;
				if (DllTest.GetOpenFileName(openFileName))
				{
					GameTerrain.Instance.LoadGeologicalMap(mineralType, openFileName.file);
				}
			}));
			Items.Add(new DebugMenuItemCustom("Save " + mineralType.ToString() + " png", delegate
			{
				OpenFileName openFileName = new OpenFileName();
				openFileName.structSize = Marshal.SizeOf(openFileName);
				openFileName.filter = "png files (*.png)\0*.png\0All files (*.*)\0*.*\0\0";
				openFileName.file = new string(new char[256]);
				openFileName.maxFile = openFileName.file.Length;
				openFileName.fileTitle = new string(new char[64]);
				openFileName.maxFileTitle = openFileName.fileTitle.Length;
				openFileName.initialDir = GameImpl.Instance.SaveGamePath;
				openFileName.title = "Save Geological Map as PNG Image";
				openFileName.defExt = "PNG";
				openFileName.flags = 526344;
				if (DllTest.GetSaveFileName(openFileName))
				{
					GameTerrain.Instance.SaveGeologicalMap(mineralType, openFileName.file);
				}
			}));
		}
		Items.Add(new DebugMenuItemCustom("Load Height png", delegate
		{
			OpenFileName openFileName = new OpenFileName();
			openFileName.structSize = Marshal.SizeOf(openFileName);
			openFileName.filter = "png files (*.png)\0*.png\0All files (*.*)\0*.*\0\0";
			openFileName.file = new string(new char[256]);
			openFileName.maxFile = openFileName.file.Length;
			openFileName.fileTitle = new string(new char[64]);
			openFileName.maxFileTitle = openFileName.fileTitle.Length;
			openFileName.initialDir = GameImpl.Instance.SaveGamePath;
			openFileName.title = "Open Height Map as PNG Image";
			openFileName.defExt = "PNG";
			openFileName.flags = 530440;
			if (DllTest.GetOpenFileName(openFileName))
			{
				GameTerrain.Instance.LoadHeightMap(openFileName.file);
			}
		}));
		Items.Add(new DebugMenuItemCustom("Save Height png", delegate
		{
			OpenFileName openFileName = new OpenFileName();
			openFileName.structSize = Marshal.SizeOf(openFileName);
			openFileName.filter = "png files (*.png)\0*.png\0All files (*.*)\0*.*\0\0";
			openFileName.file = new string(new char[256]);
			openFileName.maxFile = openFileName.file.Length;
			openFileName.fileTitle = new string(new char[64]);
			openFileName.maxFileTitle = openFileName.fileTitle.Length;
			openFileName.initialDir = GameImpl.Instance.SaveGamePath;
			openFileName.title = "Save Height Map as PNG Image";
			openFileName.defExt = "PNG";
			openFileName.flags = 526344;
			if (DllTest.GetSaveFileName(openFileName))
			{
				GameTerrain.Instance.SaveHeightMap(openFileName.file);
			}
		}));
	}
}
