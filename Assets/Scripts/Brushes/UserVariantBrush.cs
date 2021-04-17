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
using TiltBrush;
using UnityEngine;

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

  public class SubSection : Attribute {}

  /// <summary>
  /// This class is used to serialize in the brush data. Most of the fields have MapTo attributes
  /// attached, which means that they map directly to fields on BrushDescriptor.
  /// </summary>
  [Serializable]
  public class BrushProperties {
    [JsonProperty(Required = Required.Always)] public string VariantOf;
    [JsonProperty(Required = Required.Always)] public string GUID;
    [JsonProperty(Required = Required.Always)] public string ButtonIcon;
    [JsonProperty(Required = Required.Always)] [MapTo("m_DurableName")] public string Name;
    [JsonProperty(Required = Required.Always)] [MapTo("m_Description")] public string Description;
    [MapTo("m_DescriptionExtra")] [CanBeNull] public string ExtraDescription;

    [Serializable]
    public class AudioProperties {
      [CanBeNull] string[] AudioClips;
      [MapTo("m_BrushAudioMaxPitchShift")] public float? MaxPitchShift;
      [MapTo("m_BrushAudioMaxVolume")] public float? MaxVolume;
      [MapTo("m_BrushVolumeUpSpeed")] public float? VolumeUpSpeed;
      [MapTo("m_BrushVolumeDownSpeed")] public float? VolumeDownSpeed;
      [MapTo("m_VolumeVelocityRangeMultiplier")] public float? VolumeVelocityRangeMultiplier;
      [MapTo("m_AudioReactive")] public float? IsAudioReactive;
      [CanBeNull] private string ButtonAudio;
    }
    [CanBeNull] [SubSection] public AudioProperties Audio;

    [Serializable]
    public class MaterialProperties {
      [CanBeNull] public string Material;
      [CanBeNull] public string Shader;
      public Dictionary<string, int> IntProperties;
      public Dictionary<string, float> FloatProperties;
      public Dictionary<string, Color> ColorProperties;
      public Dictionary<string, string> TextureProperties;
      [MapTo("m_TextureAtlasV")] public int? TextureAtlasV;
      [MapTo("m_TileRate")] public float? TileRate;
      [MapTo("m_UseBloomSwatchOnColorPicker")] public int? UseBloomSwatchOnColorPicker;
    }
    [CanBeNull] [SubSection] public MaterialProperties Material;

    [Serializable]
    public class SizeProperties {
      [MapTo("m_BrushSizeRange")] public Vector2? BrushSizeRange;
      [MapTo("m_PressureSizeRange")] public Vector2? PressureSizeRange;
      [MapTo("m_SizeVariance")] public float? SizeVariance;
      [MapTo("m_PreviewPressureSizeMin")] public float? PreviewPressureSizeMin;
    }
    [CanBeNull] [SubSection] public SizeProperties Size;

    [Serializable]
    public class ColorProperties {
      [MapTo("m_Opacity")] public float? Opacity;
      [MapTo("m_PressureOpacityRange")] public Vector2? PressureOpacityRange;
      [MapTo("m_ColorLuminanceMin")] public Vector2? LuminanceMin;
      [MapTo("m_ColorSaturationMax")] public Vector2? SaturationMax;
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
      [MapTo("m_SizeRatio")] public Vector2? SizeRatio;
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

  private const string kNormalMapName = "_BumpMap";
  private BrushProperties m_BrushProperties;
  private string m_ConfigData;
  private Dictionary<string, byte[]> m_FileData;
  private string m_Location;
  
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
    if (brush.Initialize(brushFile)) {
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

  private bool Initialize(FolderOrZipReader brushFile) {
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

    Texture2D icon = LoadTexture(brushFile, m_BrushProperties.ButtonIcon);
    if (icon == null) {
      Debug.Log($"Brush at {m_Location} has no icon texture.");
      return false;
    }
    Descriptor.m_ButtonTexture = icon;

    CopyConfigToDescriptor(m_BrushProperties, Descriptor);
    ApplyMaterialProperties(brushFile, m_BrushProperties.Material);

    return true;
  }
  
  

  private void CopyConfigToDescriptor(System.Object propertiesObject, BrushDescriptor descriptor) {
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
        descriptorField.SetValue(descriptor, fieldValue);
      } else {
        if (field.GetCustomAttributes<SubSection>(true).Any()) {
          CopyConfigToDescriptor(fieldValue, descriptor);
        }
      }
    }
  }

  private void ApplyMaterialProperties(FolderOrZipReader brushFile,
      BrushProperties.MaterialProperties properties) {
    // TODO : Support different material
    // TODO : Support different shader
    
    Descriptor.Material = new Material(Descriptor.Material);

    if (properties.IntProperties != null) {
      foreach (var item in properties.IntProperties) {
        if (!Descriptor.Material.HasProperty(item.Key)) {
          Debug.LogError($"Material does not have property ${item.Key}.");
          continue;
        }

        Descriptor.Material.SetInt(item.Key, item.Value);
      }
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

        Descriptor.Material.SetColor(item.Key, item.Value);
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

}
