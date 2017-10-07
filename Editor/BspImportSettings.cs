using System;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class BspImportSettings
{
	public string SceneName = "";
	public string BspPath = "";
	public bool LightMap = false;
	public bool Entities = true;
	public CollidersEnum Colliders = CollidersEnum.Visuals;
	public bool Sky = false;
	public float Scale = 1/16f;

	public int LightMask = 0;
	public int Model0Layer = 0;
	public int OtherModelLayer = 0;

	public enum CollidersEnum
	{
		None = 0,
		Visuals = 1,
		ClipNodes = 2
	}

	#region Entities

	public bool LightEntities = false;
	public bool LightEntitiesEnabled = true;

	public bool BuyzoneEntity = true;
	public bool BombPlantEntity = true;

	public bool WallEntity = false;
	public bool IllusionaryEntity = false;
	public bool BreakableEntity = false;

	public bool DoorEntity = false;

	public bool PlayerSpawns = false;

	public void SetAllEntities(bool newVal)
	{
		LightEntities = newVal;
		BuyzoneEntity = newVal;
		BombPlantEntity = newVal;
		WallEntity = newVal;
		IllusionaryEntity = newVal;
		BreakableEntity = newVal;
		DoorEntity = newVal;
		PlayerSpawns = newVal;
	}

	#endregion

	#region Textures

	public string TextureLookupDirectory = "bsp_textures";

	public Color MissingTextureColor = new Color(1.0f, 0.5f, 1.0f);

	#endregion
}