using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BspLib.Bsp;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;
using System.IO;

public class BspWindow : EditorWindow
{
	[MenuItem("Window/Bsp Import")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(BspWindow));
	}

	private Vector2 _scroll = Vector2.zero;

	private bool _foldout_bspfile = true;
	private bool _foldout_scale = true;
	private bool _foldout_settings = true;
	private bool _foldout_textures = true;
	private bool _foldout_textures_missing = true;
	private bool _foldout_textures_packed = false;

	private int _popup_scale_id = 0;

	public const float Scale1hu = 1;
	public const float Scale16hu = Scale1hu / 16f;
	public const float Scale1ft = Scale16hu / 16f;
	public const float Scale1m = Scale1ft * 0.3048f;

	private string _last_bspPath = "";
	private string _bspVersion_name = "";

	public string[] PackedTextures = null;
	public string[] RequiredTextures = null;

	// The actual window code goes here
	void OnGUI()
	{
		int width = (int)position.width;
		int width1 = width - 165;

		_scroll = EditorGUILayout.BeginScrollView(_scroll);
		{
			EditorGUILayout.BeginVertical("Box");
			_foldout_bspfile = EditorGUILayout.Foldout(_foldout_bspfile, "BSP File");
			if(_foldout_bspfile)
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Scene name");
					ImportSettings.SceneName = GUILayout.TextField(ImportSettings.SceneName, 16, GUILayout.Width(width1));
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Bsp file path");
					ImportSettings.BspPath = GUILayout.TextField(ImportSettings.BspPath, GUILayout.Width(width1 - 20 - 4));
					if(GUILayout.Button("...", GUILayout.Width(20)))
					{
						var path = EditorUtility.OpenFilePanel("Select BSP file", "", "bsp");
						if(!string.IsNullOrEmpty(path))
						{
							ImportSettings.BspPath = path;
							if(string.IsNullOrEmpty(ImportSettings.SceneName))
								ImportSettings.SceneName = System.IO.Path.GetFileNameWithoutExtension(path);
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				{
					// Refresh loaded info
					if(_last_bspPath != ImportSettings.BspPath)
					{
						_last_bspPath = ImportSettings.BspPath;

						bool exists = File.Exists(_last_bspPath);

						_bspVersion_name = exists ? BspLib.Bsp.BspFile.GetVersionName(_last_bspPath) : "File Not Found";

						if(exists)
						{
							try
							{
								var bsp = new BspFile();
								BspFile.LoadPackedTexturesFromFile(bsp, _last_bspPath);

								{
									var packed = bsp.PackedGoldSourceTextures.ToArray();
									PackedTextures = new string[packed.Length];

									for(int i = 0; i < packed.Length; i++)
									{
										BspLib.Wad.Texture wt = packed[i];
										PackedTextures[i] = wt.Name;
									}
								}

								RequiredTextures = BspFile.GetUsedTextures(_last_bspPath);
							}
							catch
							{
								PackedTextures = new string[0];
								RequiredTextures = new string[0];
							}
						}
						else
						{
							PackedTextures = new string[0];
							RequiredTextures = new string[0];
						}
					}

					EditorGUILayout.LabelField("Version", _bspVersion_name);
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical("Box");
			{
				_foldout_textures = EditorGUILayout.Foldout(_foldout_textures, "Textures");
				if(_foldout_textures)
				{
					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Texture lookup directory");
						ImportSettings.TextureLookupDirectory = GUILayout.TextField(ImportSettings.TextureLookupDirectory, GUILayout.Width(width1 - 20 - 4));
						if(GUILayout.Button("...", GUILayout.Width(20)))
						{
							var path = EditorUtility.OpenFolderPanel("Select Texture lookup directory", "", ImportSettings.TextureLookupDirectory);
							if(!string.IsNullOrEmpty(path))
							{
								ImportSettings.TextureLookupDirectory = path;
							}
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Missing Texture Color");

						ImportSettings.MissingTextureColor = EditorGUILayout.ColorField(ImportSettings.MissingTextureColor);
					}
					EditorGUILayout.EndHorizontal();

                    if(RequiredTextures != null)
                    {
					    var missing = RequiredTextures.Where((t) => t != null && (PackedTextures == null || !PackedTextures.Any((texture) => texture != t)) && !File.Exists(Path.Combine(ImportSettings.TextureLookupDirectory, t+".png"))).ToArray();
					    if(missing.Any())
					    {
						    EditorGUILayout.BeginVertical("Box");
						    {
							    _foldout_textures_missing = EditorGUILayout.Foldout(_foldout_textures_missing, "Missing");
							    if(_foldout_textures_missing)
							    {
								    foreach(var m in missing)
									    GUILayout.Label(m);
							    }
						    }
						    EditorGUILayout.EndVertical();
					    }
                    }

					if(PackedTextures != null && PackedTextures.Length > 0)
					{
						EditorGUILayout.BeginVertical("Box");
						{
							_foldout_textures_packed = EditorGUILayout.Foldout(_foldout_textures_packed, "Packed Textures");
							if(_foldout_textures_packed)
							{
								foreach(string pt in PackedTextures)
									GUILayout.Label(pt);
							}
						}
						EditorGUILayout.EndVertical();
					}
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical("Box");
			{
				_foldout_scale = EditorGUILayout.Foldout(_foldout_scale, "Scale");
				if(_foldout_scale)
				{
					EditorGUILayout.BeginHorizontal();
					{
						float scale = EditorGUILayout.FloatField("Value", ImportSettings.Scale);
						if(scale != ImportSettings.Scale)
						{
							ImportSettings.Scale = scale;

							if(scale == Scale1hu)
								_popup_scale_id = 1;
							else if(scale == Scale16hu)
								_popup_scale_id = 2;
							else if(scale == Scale1ft)
								_popup_scale_id = 3;
							else if(scale == Scale1m)
								_popup_scale_id = 4;
							else
								_popup_scale_id = 0;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.PrefixLabel("Preset");
						_popup_scale_id = EditorGUILayout.Popup(_popup_scale_id, new string[]{ "<custom>", "1 hammer", "16 hammer", "1 feet", "1 meter" });
						switch(_popup_scale_id)
						{
							default:
								break;
							case 1:
								ImportSettings.Scale = Scale1hu;
								break;
							case 2:
								ImportSettings.Scale = Scale16hu;
								break;
							case 3:
								ImportSettings.Scale = Scale1ft;
								break;
							case 4:
								ImportSettings.Scale = Scale1m;
								break;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.BeginVertical("Box");
						{
							GUILayout.Label("1 in Hammer", EditorStyles.boldLabel);
							GUILayout.Label(string.Format("{0} in Unity3D", ImportSettings.Scale * 16));
							GUILayout.Label(string.Format("{0} feet", 1 / 16f));
							GUILayout.Label(string.Format("{0} meter", 1 / 16f * 0.3048f));
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical("Box");
						{
							GUILayout.Label("1 in Unity3D", EditorStyles.boldLabel);
							float hu = 16f * ImportSettings.Scale;
							GUILayout.Label(string.Format("{0} in Hammer", hu));
							GUILayout.Label(string.Format("{0} feet", hu / 16f));
							GUILayout.Label(string.Format("{0} meter", (hu / 16f) * 0.3048f));
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical("Box");
			{
				_foldout_settings = EditorGUILayout.Foldout(_foldout_settings, "Settings");
				if(_foldout_settings)
				{
					ImportSettings.Colliders = (BspImportSettings.CollidersEnum)EditorGUILayout.EnumPopup("Generate colliders", ImportSettings.Colliders);


					ImportSettings.LightMap = EditorGUILayout.Toggle("Import light map", ImportSettings.LightMap);
					ImportSettings.Sky = EditorGUILayout.Toggle("Import sky mesh", ImportSettings.Sky);

					ImportSettings.Model0Layer = EditorGUILayout.LayerField("Model 0 (solid) mask", ImportSettings.Model0Layer);
					ImportSettings.OtherModelLayer = EditorGUILayout.LayerField("Other models (entities) mask", ImportSettings.OtherModelLayer);

					EditorGUILayout.BeginVertical("Box");
					{
						ImportSettings.Entities = EditorGUILayout.ToggleLeft("Import Entities", ImportSettings.Entities);

						if(ImportSettings.Entities)
						{
							EditorGUILayout.BeginHorizontal();
							{
								if(GUILayout.Button("None", GUILayout.Width(50)))
									ImportSettings.SetAllEntities(false);
								if(GUILayout.Button("All", GUILayout.Width(50)))
									ImportSettings.SetAllEntities(true);
							}
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.Space();

							ImportSettings.LightEntities = EditorGUILayout.ToggleLeft("Light entities (light, light_environment, light_spot)", ImportSettings.LightEntities);
							if(ImportSettings.LightEntities)
							{
								ImportSettings.LightMask = EditorGUILayout.LayerField("Light mask", ImportSettings.LightMask);
								ImportSettings.LightEntitiesEnabled = EditorGUILayout.ToggleLeft("Enable light component", ImportSettings.LightEntitiesEnabled);
							}

							ImportSettings.BuyzoneEntity = EditorGUILayout.ToggleLeft("Buy zone (func_buyzone)", ImportSettings.BuyzoneEntity);
							ImportSettings.BombPlantEntity = EditorGUILayout.ToggleLeft("Bomb plant (func_bomb_target)", ImportSettings.BombPlantEntity);
							GUI.enabled = false;
							ImportSettings.WallEntity = EditorGUILayout.ToggleLeft("Wall (func_wall)", ImportSettings.WallEntity);
							ImportSettings.IllusionaryEntity = EditorGUILayout.ToggleLeft("Illusionary wall (func_illusionary)", ImportSettings.IllusionaryEntity);
							ImportSettings.BreakableEntity = EditorGUILayout.ToggleLeft("Breakable objects (func_breakable)", ImportSettings.BreakableEntity);
							ImportSettings.DoorEntity = EditorGUILayout.ToggleLeft("Doors (func_door, func_door_rotating)", ImportSettings.DoorEntity);
							ImportSettings.PlayerSpawns = EditorGUILayout.ToggleLeft("Player spawns (info_player_deathmatch/start, info_vip_start)", ImportSettings.PlayerSpawns);
							GUI.enabled = true;
						}
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();

			if(ImportSettings.Model0Layer == ImportSettings.OtherModelLayer)
				EditorGUILayout.HelpBox("It is recommended to use different layers for Model 0 and Other Models.\nOtherwise it may create light bugs or something worse.", MessageType.Warning, true);

			if(ImportSettings.LightMap && ImportSettings.LightEntities && ImportSettings.LightEntitiesEnabled)
				EditorGUILayout.HelpBox("Both Light map import and Light Entities are enabled.\nIt may create light bugs or something worse.", MessageType.Warning, true);

			if(string.IsNullOrEmpty(ImportSettings.SceneName) || !Regex.IsMatch(ImportSettings.SceneName, "^[a-zA-Z0-9_\\\\]+$"))
			{
				EditorGUILayout.HelpBox("Invalid scene name.", MessageType.Error, true);
			}
			else if(EditorSceneManager.GetSceneByName(ImportSettings.SceneName).IsValid())
			{
				EditorGUILayout.HelpBox("Name of scene is already used.", MessageType.Error, true);
			}
			else if(!System.IO.File.Exists(ImportSettings.BspPath))
			{
				EditorGUILayout.HelpBox("Map file does not exist.", MessageType.Error, true);
			}
			else if(!Application.isEditor)
			{
				EditorGUILayout.HelpBox("Availible only in editor.", MessageType.Error, true);
			}
			else if(Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Not availible while playing.", MessageType.Error, true);
			}
			else
			{
				if(GUILayout.Button("Import"))
				{
					CreateMap(ImportSettings);
				}
			}
		}
		EditorGUILayout.EndScrollView();
	}

	public BspImportSettings ImportSettings = new BspImportSettings();

	public static bool IsPowerOfTwo(int x)
	{
		return (x & (x - 1)) == 0;
	}

	public static void CreateMap(BspImportSettings settings)
	{
		//FIXME Save prev scene

		var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

		BspFile bsp = new BspFile();
		BspFile.LoadAllFromFile(bsp, BspFile.AllLoadFlags, settings.BspPath);

		// Import models
		{
			bsp.PackedGoldSourceTextures.Clear();
			BspFile.LoadPackedTexturesFromFile(bsp, settings.BspPath);
			List<BspLib.Wad.Texture> packedTextures = bsp.PackedGoldSourceTextures;
			Debug.LogFormat("{0} packed textures", packedTextures.Count);

			var go_all_models = new GameObject("Models");
			for(int i = 0; i < bsp.Models.Count; i++)
			{
				var model = bsp.Models[i];
				var go = new GameObject("" + i);
				go.transform.SetParent(go_all_models.transform);
				if(i == 0)
					go.isStatic = true;
				go.layer = i == 0 ? settings.Model0Layer : settings.OtherModelLayer;

				// Count triangles
				bool hasSkybox = false;
				int submeshCountWithoutSky = 0;
				System.Collections.Generic.KeyValuePair<string, uint[]> kvp_sky = default(KeyValuePair<string, uint[]>);
				foreach(var kvp in model.Triangles)
				{
					if(kvp.Key == "sky")
					{
						hasSkybox = true;
						kvp_sky = kvp;
					}
					else
						submeshCountWithoutSky++;
				}

				// Create Mesh
				var mesh = new Mesh();
				mesh.name = string.Format("Model {0}", i);
				// Submesh Count
				mesh.subMeshCount = submeshCountWithoutSky;
				//mesh_renderer.materials = new Material[model.Triangles.Count];

				// Create vertices and uv
				var vertices = new List<Vector3>();
				var uv = new List<Vector2>();
				for(int ii = 0; ii < model.Positions.Length; ii++)
				{
					var p = model.Positions[ii];
					vertices.Add(new Vector3(p.x * settings.Scale, p.z * settings.Scale, p.y * settings.Scale));

					var t = model.TextureCoordinates[ii];
					uv.Add(new Vector2(t.x, -t.y));
				}
				mesh.SetVertices(vertices);
				mesh.SetUVs(0, uv);

				var materials = new Material[submeshCountWithoutSky];

				int submesh = 0;
				foreach(var kvp in model.Triangles)
				{
					if(kvp.Key == "sky")
						continue;

					var indices = new int[kvp.Value.Length];
					for(int ii = 0; ii < indices.Length; ii += 3)
					{
						indices[ii + 0] = (int)kvp.Value[ii + 0];
						indices[ii + 1] = (int)kvp.Value[ii + 1];
						indices[ii + 2] = (int)kvp.Value[ii + 2];
					}
					mesh.SetTriangles(indices, submesh: submesh);

					// Add material
					{								
						var material = new Material(Shader.Find("Diffuse"));
						material.name = kvp.Key;

						//Texture2D texture = settings.AdditionalTextures == null ? null : settings.AdditionalTextures.FirstOrDefault((t) => t != null && string.Equals(t.name, kvp.Key, System.StringComparison.InvariantCultureIgnoreCase));
						Texture2D texture = null;
						foreach(var pt in packedTextures)
						{
							if(pt.Name == kvp.Key)
							{
								texture = pt.Bitmap;
								material.name = string.Format("{0} (PACKED IN BSP)", material.name);
								break;
							}
						}
						if(texture == null)
						{
							var texture_path = Path.Combine(settings.TextureLookupDirectory, kvp.Key + ".png");
							if(File.Exists(texture_path))
							{
								var data = File.ReadAllBytes(texture_path);
								texture = new Texture2D(1, 1);
								texture.LoadImage(data);
							}
							else
							{
								Debug.LogWarning(string.Format("Texture '{0}' could not be found. '{1}'", kvp.Key, Path.GetFullPath(texture_path)));
								material.name = string.Format("{0} (MISSING FILE)", material.name);
								material.color = settings.MissingTextureColor;
							}
						}

						material.mainTexture = texture;

						materials[submesh] = material;
					}

					submesh++;
				}

				mesh.RecalculateBounds();
				mesh.RecalculateNormals();

				// Create Mesh Renderer
				MeshRenderer mesh_renderer = go.AddComponent<MeshRenderer>();
				mesh_renderer.materials = materials;

				// Create Mesh Filter
				MeshFilter mesh_filter = go.AddComponent<MeshFilter>();
				mesh_filter.sharedMesh = mesh;

				// Crete Mesh Collider
				if(settings.Colliders == BspImportSettings.CollidersEnum.Visuals)
				{
					MeshCollider mesh_collider = go.AddComponent<MeshCollider>();
					mesh_collider.sharedMesh = mesh;
				}

				if(hasSkybox && settings.Sky)
				{
					var sky_go = new GameObject("Skybox");
					sky_go.transform.SetParent(go.transform);

					var sky_mesh = new Mesh();
					sky_mesh.name = string.Format("Model {0} Skybox", i);

					// Create vertices and uv
					var sky_vertices = new List<Vector3>();
					for(int ii = 0; ii < model.Positions.Length; ii++)
					{
						var p = model.Positions[ii];
						sky_vertices.Add(new Vector3(p.x * settings.Scale, p.z * settings.Scale, p.y * settings.Scale));
					}
					sky_mesh.SetVertices(vertices);


					var sky_indices = new int[kvp_sky.Value.Length];
					for(int ii = 0; ii < sky_indices.Length; ii += 3)
					{
						sky_indices[ii + 0] = (int)kvp_sky.Value[ii + 0];
						sky_indices[ii + 1] = (int)kvp_sky.Value[ii + 1];
						sky_indices[ii + 2] = (int)kvp_sky.Value[ii + 2];
					}
					sky_mesh.SetTriangles(sky_indices, submesh: 0);

					var sky_mesh_filter = sky_go.AddComponent<MeshFilter>();
					sky_mesh_filter.sharedMesh = sky_mesh;

					var sky_mesh_renderer = sky_go.AddComponent<MeshRenderer>();
					{						
						var sky_material = new Material(Shader.Find("Diffuse"));
						sky_material.name = "Skybox";

						//TODO load image from file
						sky_material.color = Color.cyan;																																																																																													
						/*
						var sky_texture = Resources.Load<Texture2D>("textures/sky");
						if(sky_texture == null)
						{
							sky_texture = new Texture2D(1, 1);
							sky_texture.SetPixel(0, 0, Color.cyan);
						}
						sky_material.mainTexture = sky_texture;
						*/

						sky_mesh.RecalculateBounds();
						sky_mesh.RecalculateNormals();

						sky_mesh_renderer.sharedMaterial = sky_material;
					}

					if(settings.Colliders == BspImportSettings.CollidersEnum.Visuals)
					{
						var sky_mesh_collider = sky_go.AddComponent<MeshCollider>();
						sky_mesh_collider.sharedMesh = sky_mesh;
					}
				}

				if(settings.Colliders == BspImportSettings.CollidersEnum.ClipNodes)
				{
					// model.
					//TODO collision
				}
			}
		}

		// Import Entities
		if(settings.Entities)
		{
			var go_entities = new GameObject("Entities");
			foreach(var e in bsp.Entities)
			{
				string classname;
				if(!e.TryGetValue("classname", out classname))
				{
					Debug.LogError("Error during BSP import. Entity does not contain 'classname' tag. Probably not valid entity. Skipping.");
					continue;
				}

				var go = new GameObject(classname);
				go.transform.SetParent(go_entities.transform);

				{
					var raw = go.AddComponent<RawEntity>();
					foreach(var kvp in e)
						raw.Text += string.Format("\"{0}\" \"{1}\"\n", kvp.Key, kvp.Value);
				}

				if(e.ContainsKey("origin"))
				{
					string[] spl = e["origin"].Trim().Split(' ');
					if(spl.Length == 3)
					{
						float x, y, z;
						if(float.TryParse(spl[0], out x) && float.TryParse(spl[1], out y) && float.TryParse(spl[2], out z))
						{
							go.transform.position = new Vector3(x * settings.Scale, z * settings.Scale, y * settings.Scale);
						}
					}
				}

				if(e.ContainsKey("model"))
				{
					var m = e["model"];
					if(m.StartsWith("*"))
					{
						int m_index;
						if(int.TryParse(m.Substring(1), out m_index))
						{
							var m_go = GameObject.Find("Models/" + m_index);
							if(m_go == null)
							{
								Debug.LogWarningFormat("Model {0} for {1} could not be found", m_index, classname);
							}
							else
							{
								var curr_m_go = Object.Instantiate(m_go);// Clone
								curr_m_go.transform.SetParent(go.transform);

								m_go.SetActive(false);
							}
						}
						else
						{
							Debug.LogWarningFormat("Invalid model number {0} for {1}", m_index, classname);
						}										
					}
				}

				switch(classname)
				{
					default:
						Debug.LogFormat("Unknown / unsupported entity '{0}'.", classname);
						break;
					case "light":
					case "light_environment":
						if(!settings.LightEntities)
							break;

						var light = go.AddComponent<Light>();
						light.type = classname == "light" ? LightType.Point : LightType.Directional;
						light.enabled = settings.LightEntitiesEnabled;

						if(e.ContainsKey("angle"))
						{
							int angle;
							if(int.TryParse(e["angle"], out angle))
							{
								light.transform.eulerAngles += new Vector3(angle, 0, 0);
							}
						}

						if(e.ContainsKey("_light"))
						{
							string[] _light = e["_light"].Trim().Split(' ');

							if(_light.Length == 4)
							{
								int l_brightness;
								if(int.TryParse(_light[3], out l_brightness))
								{
									light.intensity = 1;
									light.range = l_brightness * settings.Scale * 16;// probably in ft
								}
							}

							if(_light.Length == 3 || _light.Length == 4)
							{
								byte l_r, l_g, l_b;
								if(byte.TryParse(_light[0], out l_r) && byte.TryParse(_light[1], out l_g) && byte.TryParse(_light[2], out l_b))
								{
									light.color = new Color(l_r / 255f, l_g / 255f, l_b / 255f);
								}

								light.cullingMask = settings.LightMask;
							}
						}
						break;
					case "func_buyzone":
						{
							if(!settings.BuyzoneEntity)
								break;
							var buyzone = go.AddComponent<BuyZone>();
							int team;
							if(int.TryParse(e["team"], out team))
								buyzone.Team = team;

							foreach(var mr in go.GetComponents<MeshRenderer>())
								mr.enabled = false;
							foreach(Transform t in go.transform)
								foreach(var mr in t.gameObject.GetComponents<MeshRenderer>())
									mr.enabled = false;

							foreach(var mc in go.GetComponents<MeshCollider>())
								mc.isTrigger = true;
							foreach(Transform t in go.transform)
								foreach(var mc in t.gameObject.GetComponents<MeshCollider>())
									mc.isTrigger = true;
						}
						break;
					case "func_bomb_target":
						{
							if(!settings.BombPlantEntity)
								break;

							var buyzone = go.AddComponent<BombTarget>();

							foreach(var mr in go.GetComponents<MeshRenderer>())
								mr.enabled = false;
							foreach(Transform t in go.transform)
								foreach(var mr in t.gameObject.GetComponents<MeshRenderer>())
									mr.enabled = false;

							foreach(var mc in go.GetComponents<MeshCollider>())
								mc.isTrigger = true;
							foreach(Transform t in go.transform)
								foreach(var mc in t.gameObject.GetComponents<MeshCollider>())
									mc.isTrigger = true;
						}
						break;
                    case "func_tracktrain":
                        {
                            Debug.Log("Found train");
                        }
                        break;

                }

			}
		}

		// Colliders
		if(bsp.Colliders.Count > 0)
		{
			var go_colliders = new GameObject("Colliders");
			for(int i = 0; i < bsp.Colliders.Count; i++)
			{
				var m_source = bsp.Colliders[i];

				var m = new Mesh();
				m.name = "Collision Mesh " + i;

				// scale + transform vertices
				{
					var vertices_source = m_source.vertices;
					var vertices = new Vector3[vertices_source.Length];
					for(int vi = 0; vi < vertices_source.Length; vi++)
					{
						var v = vertices_source[vi];
						vertices[vi] = new Vector3(v.x * settings.Scale, v.z * settings.Scale, v.y * settings.Scale);
					}
					m.vertices = vertices;
				}
				m.SetIndices(m_source.GetIndices(0), m_source.GetTopology(0), 0);

				var go = new GameObject(i.ToString());
				go.transform.SetParent(go_colliders.transform);

				var collider = go.AddComponent<MeshCollider>();
				collider.sharedMesh = m;
			}
		}
	}
}