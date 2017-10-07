using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BspLib.Bsp;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;
using System.IO;
using BspLib.Wad;

public class WadWindow : EditorWindow
{
	[MenuItem("Window/Textures from Wad")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(WadWindow));
	}

	private Vector2 _scroll = Vector2.zero;

	private bool _foldout_paths = true;

	public WadImportSettings ImportSettings = new WadImportSettings();

	// The actual window code goes here
	void OnGUI()
	{
		if(ImportSettings == null)
			ImportSettings = new WadImportSettings();

		int width = (int)position.width;
		int width1 = width - 165;

		_scroll = EditorGUILayout.BeginScrollView(_scroll);
		{
			EditorGUILayout.BeginVertical("Box");
			_foldout_paths = EditorGUILayout.Foldout(_foldout_paths, "Paths");
			if(_foldout_paths)
			{

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Wad file path");
					ImportSettings.WadPath = GUILayout.TextField(ImportSettings.WadPath, GUILayout.Width(width1 - 20 - 4));
					if(GUILayout.Button("...", GUILayout.Width(20)))
					{
						var path = EditorUtility.OpenFilePanel("Select WAD file", "", "wad");
						if(!string.IsNullOrEmpty(path))
						{
							ImportSettings.WadPath = path;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Texture export path");
					ImportSettings.SaveDirectory = GUILayout.TextField(ImportSettings.SaveDirectory, GUILayout.Width(width1 - 20 - 4));
					if(GUILayout.Button("...", GUILayout.Width(20)))
					{
						var path = EditorUtility.OpenFolderPanel("Select target directory", "", "textures");
						if(!string.IsNullOrEmpty(path))
						{
							ImportSettings.SaveDirectory = path;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

			}
			EditorGUILayout.EndVertical();

			if(Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Not availible while playing.", MessageType.Error, true);
			}
			else if(!File.Exists(ImportSettings.WadPath))
			{
				EditorGUILayout.HelpBox("Wad file does not exist.", MessageType.Error, true);
			}
			else if(!Directory.Exists(ImportSettings.SaveDirectory))
			{
				EditorGUILayout.HelpBox("Target directory does not exist.", MessageType.Error, true);
			}
			else
			{
				if(GUILayout.Button("Import"))
				{
					ImportTextures(ImportSettings);
				}
			}
		}
		EditorGUILayout.EndScrollView();
	}

	public static void ImportTextures(WadImportSettings settings)
	{
		var wad = new WadFile();
		WadFile.Load(wad, settings.WadPath);

		foreach(var t in wad.Textures)
		{
			string path = Path.Combine(settings.SaveDirectory, t.Name + ".png");
			File.WriteAllBytes(path, t.Bitmap.EncodeToPNG());
		}
	}
}