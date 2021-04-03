﻿// Copyright 2020 The Open Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TiltBrush;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
/// A variant Brush based on an existing brush, but with different:
/// * Texture
/// * Icon
/// * Name
/// * (Optional) Sound
/// </summary>
[Serializable]
public class UserVariantBrush {
  public const string kConfigFile = "Brush.cfg";

  public class MapTo : Attribute {
    public string FieldName { get; set; }
    public MapTo(string fieldName) {
      FieldName = fieldName;
    }
  }

  public enum CopyRestrictions {
    EmbedAndShare,
    EmbedAndDoNotShare,
    DoNotEmbed
  }

  public class SubSection : Attribute {}

  /// <summary>
  /// This class is used to serialize in the brush data. Most of the fields have MapTo attributes
  /// attached, which means that they map directly to fields on BrushDescriptor.
  /// </summary>
  [Serializable]
  public class BrushProperties {
    [JsonProperty(Required = Required.Always)] public string VariantOf;
    [JsonProperty(Required = Required.Always)] public string GUID;
    public string Author;
    [JsonProperty(Required = Required.Always)] [MapTo("m_DurableName")] public string Name;
    [JsonProperty(Required = Required.Always)] [MapTo("m_Description")] public string Description;
    [MapTo("m_DescriptionExtra")] [CanBeNull] public string ExtraDescription;
    [JsonConverter(typeof(StringEnumConverter))] public CopyRestrictions CopyRestrictions;
    public string ButtonIcon;

    [Serializable]
    public class AudioProperties {
      [CanBeNull] string[] AudioClips;
      [MapTo("m_BrushAudioMaxPitchShift")] public float? MaxPitchShift;
      [MapTo("m_BrushAudioMaxVolume")] public float? MaxVolume;
      [MapTo("m_BrushVolumeUpSpeed")] public float? VolumeUpSpeed;
      [MapTo("m_BrushVolumeDownSpeed")] public float? VolumeDownSpeed;
      [MapTo("m_VolumeVelocityRangeMultiplier")] public float? VolumeVelocityRangeMultiplier;
      [MapTo("m_AudioReactive")] public bool? IsAudioReactive;
      [CanBeNull] private string ButtonAudio;
    }
    [CanBeNull] [SubSection] public AudioProperties Audio;

    [Serializable]
    public class MaterialProperties {
      [CanBeNull] public string Shader;
      public Dictionary<string, float> FloatProperties;
      public Dictionary<string, float[]> ColorProperties;
      public Dictionary<string, float[]> VectorProperties;
      public Dictionary<string, string> TextureProperties;
      [MapTo("m_TextureAtlasV")] public int? TextureAtlasV;
      [MapTo("m_TileRate")] public float? TileRate;
      [MapTo("m_UseBloomSwatchOnColorPicker")] public bool? UseBloomSwatchOnColorPicker;
    }
    [CanBeNull] [SubSection] public MaterialProperties Material;

    [Serializable]
    public class SizeProperties {
      [MapTo("m_BrushSizeRange")] public float[] BrushSizeRange;
      [MapTo("m_PressureSizeRange")] public float[] PressureSizeRange;
      [MapTo("m_SizeVariance")] public float? SizeVariance;
      [MapTo("m_PreviewPressureSizeMin")] public float? PreviewPressureSizeMin;
    }
    [CanBeNull] [SubSection] public SizeProperties Size;

    [Serializable]
    public class ColorProperties {
      [MapTo("m_Opacity")] public float? Opacity;
      [MapTo("m_PressureOpacityRange")] public float[] PressureOpacityRange;
      [MapTo("m_ColorLuminanceMin")] public float? LuminanceMin;
      [MapTo("m_ColorSaturationMax")] public float? SaturationMax;
    }
    [CanBeNull] [SubSection] public ColorProperties Color;

    [Serializable]
    public class ParticleProperties {
      [MapTo("m_ParticleSpeed")] public float? Speed;
      [MapTo("m_ParticleRate")] public float? Rate;
      [MapTo("m_ParticleInitialRotationRange")] public float? InitialRotationRange;
      [MapTo("m_RandomizeAlpha")] public bool? RandomizeAlpha;
    }
    [CanBeNull] [SubSection] public ParticleProperties Particle;

    [Serializable]
    public class QuadBatchProperties {
      [MapTo("m_SprayRateMultiplier")] public float? SprayRateMultiplier;
      [MapTo("m_RotationVariance")] public float? RotationVariance;
      [MapTo("m_PositionVariance")] public float? PositionVariance;
      [MapTo("m_SizeRatio")] public float[] SizeRatio;
    }
    [CanBeNull] [SubSection] public QuadBatchProperties QuadBatch;

    [Serializable]
    public class TubeProperties {
      [MapTo("m_SolidMinLengthMeters_PS")] public float? MinLength;
      [MapTo("m_TubeStoreRadiusInTexcoord0Z")] public bool? StoreRadiusInTexCoord;
    }
    [CanBeNull] [SubSection] public TubeProperties Tube;

    [Serializable]
    public class MiscProperties {
      [MapTo("m_RenderBackfaces")] public bool? RenderBackFaces;
      [MapTo("m_BackIsInvisible")] public bool? BackIsInvisible;
      [MapTo("m_BackfaceHueShift")] public float? BackfaceHueShift;
      [MapTo("m_BoundsPadding")] public float? BoundsPadding;
      [MapTo("m_PlayBackAtStrokeGranularity")] public bool? PlaybackAtStrokeGranularity;
    }
    [CanBeNull] [SubSection] public MiscProperties Misc;

    [Serializable]
    public class ExportProperties {
      [MapTo("m_EmissiveFactor")] public float? EmissiveFactor;
      [MapTo("m_AllowExport")] public bool? AllowExport;
    }
    [CanBeNull] [SubSection] public ExportProperties Export;

    [Serializable]
    public class SimplificationProperties {
      [MapTo("m_SupportsSimplification")] public bool? SupportsSimplification;
      [MapTo("m_HeadMinPoints")] public int? HeadMinPoints;
      [MapTo("m_HeadPointStep")] public int? HeadPointStep;
      [MapTo("m_TailMinPoints")] public int? TailMinPoints;
      [MapTo("m_TailPointStep")] public int? TailPointStep;
      [MapTo("m_MiddlePointStep")] public int? MiddlePointStep;
    }
    [CanBeNull] [SubSection] public SimplificationProperties Simplification;
  }

  public BrushDescriptor Descriptor { get; private set; } = null;
  public bool ShowInGUI => m_ShowInGUI;
  public bool EmbedInSketch => m_EmbedInSketch;

  private const string kNormalMapName = "_BumpMap";
  private BrushProperties m_BrushProperties;
  private string m_ConfigData;
  private Dictionary<string, byte[]> m_FileData;
  private string m_Location;
  private bool m_ShowInGUI;
  private bool m_EmbedInSketch;
  
  private UserVariantBrush() {}

  public static UserVariantBrush Create(string sourceFolder) {
    var brush = new UserVariantBrush();
    brush.m_Location = Path.GetFileName(sourceFolder);
    FolderOrZipReader brushFile = new FolderOrZipReader(sourceFolder);
    if (brushFile.IsZip) {
      string configDir = brushFile.Find(kConfigFile);
      if (configDir == null) {
        return null;
      }

      brushFile.SetRootFolder(Path.GetDirectoryName(configDir));
    }
    if (brush.Initialize(brushFile, forceInGui: true)) {
      return brush;
    }
    return null;
  }

  public static UserVariantBrush Create(SceneFileInfo fileInfo, string subfolder) {
    var brush = new UserVariantBrush();
    brush.m_Location = fileInfo.FullPath;
    FolderOrZipReader brushFile = new FolderOrZipReader(fileInfo.FullPath);
    brushFile.SetRootFolder(subfolder);
    string configDir = brushFile.Find(kConfigFile);
    if (configDir == null) {
      return null;
    }
    brushFile.SetRootFolder(Path.GetDirectoryName(configDir));
    if (brush.Initialize(brushFile)) {
      return brush;
    }
    return null;
  }

  private bool Initialize(FolderOrZipReader brushFile, bool forceInGui = false) {
    m_FileData = new Dictionary<string, byte[]>();
     if (!brushFile.Exists(kConfigFile)) {
      return false;
    }

    string warning;
    try {
      var fileReader = new StreamReader(brushFile.GetReadStream(kConfigFile));
      m_ConfigData = fileReader.ReadToEnd();
      m_BrushProperties = App.DeserializeObjectWithWarning<BrushProperties>(m_ConfigData, out warning);
    } catch (JsonException e) {
      Debug.Log($"Error reading {m_Location}/{kConfigFile}: {e.Message}");
      return false;
    }

    if (!string.IsNullOrEmpty(warning)) {
      Debug.Log($"Could not load brush at {m_Location}\n{warning}");
      return false;
    }

    var baseBrush = App.Instance.m_Manifest.Brushes.FirstOrDefault(
      x => x.m_Guid.ToString() == m_BrushProperties.VariantOf);
    if (baseBrush == null) {
      baseBrush = App.Instance.m_Manifest.Brushes.FirstOrDefault(
        x => x.m_Description == m_BrushProperties.VariantOf);
    }

    if (baseBrush == null) {
      Debug.Log(
        $"In brush at {m_Location}, no brush named {m_BrushProperties.VariantOf} could be found.");
    }

    if (App.Instance.m_Manifest.UniqueBrushes().Any(x => x.m_Guid.ToString() == m_BrushProperties.GUID)) {
      Debug.Log(
        $"Cannot load brush at {m_Location} because its GUID matches {baseBrush.name}.");
    }

    Descriptor = UnityEngine.Object.Instantiate(baseBrush);
    Descriptor.m_Guid = new SerializableGuid(m_BrushProperties.GUID);
    Descriptor.BaseGuid = baseBrush.m_Guid;
    Descriptor.name = m_BrushProperties.Name;
    Descriptor.IsUserVariant = true;
    Descriptor.m_Supersedes = null;
    Descriptor.m_SupersededBy = null;
    Descriptor.m_HiddenInGui = !forceInGui &&
                               m_BrushProperties.CopyRestrictions != CopyRestrictions.EmbedAndShare;
    m_EmbedInSketch = m_BrushProperties.CopyRestrictions != CopyRestrictions.DoNotEmbed;

    if (m_BrushProperties.ButtonIcon != null) {
      Texture2D icon = LoadTexture(brushFile, m_BrushProperties.ButtonIcon);
      if (icon == null) {
        Debug.Log($"Brush at {m_Location} has no icon texture.");
        return false;
      }
      Descriptor.m_ButtonTexture = icon;
    }

    CopyPropertiesToDescriptor(m_BrushProperties, Descriptor);
    ApplyMaterialProperties(brushFile, m_BrushProperties.Material);

    return true;
  }

  private void CopyPropertiesToDescriptor(System.Object propertiesObject, BrushDescriptor descriptor) {
    foreach (FieldInfo field in propertiesObject.GetType().GetFields()) {
      object fieldValue = field.GetValue(propertiesObject);
      if (fieldValue == null) {
        continue;
      }
      MapTo mapTo = field.GetCustomAttributes<MapTo>(true).FirstOrDefault(); 
      if (mapTo != null) {
        FieldInfo descriptorField = typeof(BrushDescriptor).GetField(mapTo.FieldName);
        if (descriptorField == null) {
          Debug.LogError(
            $"Tried to set a value {mapTo.FieldName} on BrushDescriptor, but it doesn't exist!");
          continue;
        }

        if (descriptorField.FieldType == typeof(Vector2)) {
          float[] floatArray = fieldValue as float[];
          Vector2 vector = new Vector2(floatArray[0], floatArray[1]);
          descriptorField.SetValue(descriptor, vector);
        } else {
          descriptorField.SetValue(descriptor, fieldValue);
        }
      } else {
        if (field.GetCustomAttributes<SubSection>(true).Any()) {
          CopyPropertiesToDescriptor(fieldValue, descriptor);
        }
      }
    }
  }

  private void ApplyMaterialProperties(FolderOrZipReader brushFile,
      BrushProperties.MaterialProperties properties) {
    if (properties.Shader != null) {
      Shader shader = Shader.Find(properties.Shader);
      if (shader != null) {
        Descriptor.Material = new Material(shader);
      } else {
        Debug.LogError($"Cannot find shader {properties.Shader}.");
      }
    } 
    if (Descriptor.Material == null) {
      Descriptor.Material = new Material(Descriptor.Material);
    }

    if (properties.FloatProperties != null) {
      foreach (var item in properties.FloatProperties) {
        if (!Descriptor.Material.HasProperty(item.Key)) {
          Debug.LogError($"Material does not have property ${item.Key}.");
          continue;
        }

        Descriptor.Material.SetFloat(item.Key, item.Value);
      }
    }

    if (properties.ColorProperties != null) {
      foreach (var item in properties.ColorProperties) {
        if (!Descriptor.Material.HasProperty(item.Key)) {
          Debug.LogError($"Material does not have property ${item.Key}.");
          continue;
        }

        if (item.Value.Length != 4) {
          Debug.LogError($"Color value {item.Key} in Material does not have four values.");
          continue;
        } 
        Color color = new Color(item.Value[0], item.Value[1], item.Value[2], item.Value[3]);
        Descriptor.Material.SetColor(item.Key, color);
      }
    }
    
    if (properties.VectorProperties != null) {
      foreach (var item in properties.VectorProperties) {
        if (!Descriptor.Material.HasProperty(item.Key)) {
          Debug.LogError($"Material does not have property ${item.Key}.");
          continue;
        }

        if (item.Value.Length != 4) {
          Debug.LogError($"Vector value {item.Key} in Material does not have four values.");
          continue;
        } 
        Vector4 vector = new Vector4(item.Value[0], item.Value[1], item.Value[2], item.Value[3]);
        Descriptor.Material.SetVector(item.Key, vector);
      }
    }

    if (properties.TextureProperties != null) {
      foreach (var item in properties.TextureProperties) {
        if (!Descriptor.Material.HasProperty(item.Key)) {
          Debug.LogError($"Material does not have property ${item.Key}.");
          continue;
        }
        Texture2D texture = LoadTexture(brushFile, item.Value);
        if (texture != null) {
          Descriptor.Material.SetTexture(item.Key, texture);
        } else {
          Debug.LogError($"Couldn't load texture {item.Value} for material property {item.Key}.");
        }
      }
    }
  }

  private Texture2D LoadTexture(FolderOrZipReader brushFile, string filename) {
    if (brushFile.Exists(filename)) {
      Texture2D texture = new Texture2D(16, 16);
      var buffer = new MemoryStream();
      brushFile.GetReadStream(filename).CopyTo(buffer);
      byte[] data = buffer.ToArray();
      m_FileData[filename] = data;
      if (ImageConversion.LoadImage(texture, data, true)) {
        return texture;
      }
    }
    return null;
  }

  public void Save(AtomicWriter writer, string subfolder) {
    string configPath = Path.Combine(subfolder, Path.Combine(m_Location, kConfigFile));
    using (var configStream = new StreamWriter(writer.GetWriteStream(configPath))) {
      configStream.Write(m_ConfigData);
    }

    foreach (var item in m_FileData) {
      string path = Path.Combine(subfolder, Path.Combine(m_Location, item.Key));
      using (var dataWriter = writer.GetWriteStream(path)) {
        dataWriter.Write(item.Value, 0, item.Value.Length);
      }
    }
  }
  
#if UNITY_EDITOR  
  [MenuItem("Tilt/Brushes/Export Standard Brush Properties")]
  public static void ExportDescriptorDetails() {
    TiltBrushManifest manifest =
      AssetDatabase.LoadAssetAtPath<TiltBrushManifest>("Assets/Manifest.asset");

    string destination = Path.GetFullPath(
      Path.Combine(Application.dataPath, "../Support/Brushes/ExportedProperties"));
    if (!Directory.Exists(destination)) {
      Directory.CreateDirectory(destination);
    }

    foreach (var brush in manifest.Brushes) {
      ExportDescriptor(brush, Path.Combine(destination, brush.name + ".txt"));
    }

    Debug.Log($"Exported {manifest.Brushes.Length} brushes.");
  }
  
  public static void ExportDescriptor(BrushDescriptor brush, string filename) {
    BrushProperties properties = new BrushProperties();
    properties.VariantOf = "";
    properties.GUID = brush.m_Guid.ToString();
    properties.ButtonIcon = "blank.png";
    properties.Author = "Open Brush";
    properties.CopyRestrictions = CopyRestrictions.EmbedAndShare;

    CopyDescriptorToProperties(brush, properties);
    CopyMaterialToProperties(brush, properties);

    try {
      var serializer = JsonSerializer.Create(new JsonSerializerSettings() { 
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
      serializer.ContractResolver = new CustomJsonContractResolver();
      using (var writer = new CustomJsonWriter(new StreamWriter(filename))) {
        writer.Formatting = Formatting.Indented;
        serializer.Serialize(writer, properties);
      }
    } catch (JsonException e) {
      Debug.LogWarning(e.Message);
    }
  }
  
  private static System.Object ConvertStructsToArrays(System.Object obj) {
    if (obj.GetType() == typeof(Vector2)) {
      Vector2 vector = (Vector2)obj;
      obj = new []{vector.x, vector.y};
    } else if (obj.GetType() == typeof(Color)) {
      Color color = (Color) obj;
      obj = new []{color.r, color.g, color.b, color.a};
    }

    return obj;
  }
  
  private static void CopyDescriptorToProperties(BrushDescriptor descriptor, 
                                                 System.Object propertiesObject) {
    foreach (FieldInfo field in propertiesObject.GetType().GetFields()) {
      try {
        MapTo mapTo = field.GetCustomAttributes<MapTo>(true).FirstOrDefault();
        if (mapTo != null) {
          FieldInfo descriptorField = typeof(BrushDescriptor).GetField(mapTo.FieldName);
          if (descriptorField == null) {
            Debug.LogError(
              $"Tried to set a value {mapTo.FieldName} on BrushDescriptor, but it doesn't exist!");
            continue;
          }

          System.Object fieldValue = ConvertStructsToArrays(descriptorField.GetValue(descriptor));
          field.SetValue(propertiesObject, fieldValue);
        } else {
          if (field.GetCustomAttributes<SubSection>(true).Any()) {
            System.Object fieldValue = field.GetValue(propertiesObject);
            if (fieldValue == null) {
              fieldValue = ConvertStructsToArrays(Activator.CreateInstance(field.FieldType));
              field.SetValue(propertiesObject, fieldValue);
            }

            CopyDescriptorToProperties(descriptor, fieldValue);
          }
        }
      } catch (ArgumentException e) {
        Debug.LogError($"Trying to convert ${field.Name}. {e.Message}");
        throw;
      }
    }
  }

  private static void CopyMaterialToProperties(BrushDescriptor descriptor,
     BrushProperties properties) {
    Material material = descriptor.Material;
    properties.Material.Shader = material.shader.name;

    properties.Material.FloatProperties = new Dictionary<string, float>();
    properties.Material.ColorProperties = new Dictionary<string, float[]>();
    properties.Material.VectorProperties = new Dictionary<string, float[]>();
    properties.Material.TextureProperties = new Dictionary<string, string>();

    Shader shader = material.shader;
    
    for (int i = 0; i < shader.GetPropertyCount(); ++i) {
      string name = shader.GetPropertyName(i);
      switch (shader.GetPropertyType(i)) {
        case ShaderPropertyType.Float:
        case ShaderPropertyType.Range:
          properties.Material.FloatProperties.Add(name, material.GetFloat(name));
          break;
        case ShaderPropertyType.Color:
          Color color = material.GetColor(name);
          float[] colorArray = {color.r, color.g, color.b, color.a};
          properties.Material.ColorProperties.Add(name, colorArray);
          break;
        case ShaderPropertyType.Vector:
          Vector4 vector = material.GetVector(name);
          float[] floatArray = {vector.x, vector.y, vector.z, vector.w};
          properties.Material.VectorProperties.Add(name, floatArray);
          break;
        case ShaderPropertyType.Texture:
          properties.Material.TextureProperties.Add(name, "");
          break;
        default:
          Debug.LogWarning($"Shader {shader.name} from material {material.name} has property {name} of unsupported type {shader.GetPropertyType(i)}.");
          break;
      }
    }
  }
#endif
}
