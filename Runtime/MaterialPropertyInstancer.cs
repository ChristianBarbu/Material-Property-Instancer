using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaterialPropertyInstancer
{
    /// <summary>
    /// Allows per-instance material property control while optionally sharing properties between instances with the same ID.
    /// Attach this component to any GameObject with a renderer.
    /// </summary>
    [AddComponentMenu("Rendering/Material Property Instance")]
    [RequireComponent(typeof(Renderer))]
    public class MaterialPropertyInstance : MonoBehaviour
    {
        [Tooltip("Unique identifier. Objects with the same ID share property values.")]
        [SerializeField] private string instanceId = "default";

        [Tooltip("Properties to control independently.")]
        [SerializeField] private List<PropertyDefinition> controlledProperties = new List<PropertyDefinition>();

        [Tooltip("Should properties be initialized on start with values from the material?")]
        [SerializeField] private bool initializeFromMaterial = true;

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;
        private static Dictionary<string, MaterialPropertyBlock> sharedPropertyBlocks = new Dictionary<string, MaterialPropertyBlock>();
        private static Dictionary<string, HashSet<MaterialPropertyInstance>> instanceGroups = new Dictionary<string, HashSet<MaterialPropertyInstance>>();

        // Property definition for inspector and configuration
        [Serializable]
        public class PropertyDefinition
        {
            public enum PropertyType
            {
                Float,
                Color,
                Vector4,
                Integer,
                Texture
            }

            public string name;
            public PropertyType type;
            
            // Default values for different types
            public float floatValue;
            public Color colorValue = Color.white;
            public Vector4 vectorValue;
            public int intValue;
            public Texture textureValue;
        }

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            InitializeInstance();
        }

        private void OnEnable()
        {
            // Register this instance with its group
            if (!instanceGroups.ContainsKey(instanceId))
            {
                instanceGroups[instanceId] = new HashSet<MaterialPropertyInstance>();
            }
            instanceGroups[instanceId].Add(this);
            
            // Apply current property values
            ApplyPropertyBlock();
        }

        private void OnDisable()
        {
            // Unregister this instance from its group
            if (instanceGroups.ContainsKey(instanceId))
            {
                instanceGroups[instanceId].Remove(this);
                if (instanceGroups[instanceId].Count == 0)
                {
                    instanceGroups.Remove(instanceId);
                    if (sharedPropertyBlocks.ContainsKey(instanceId))
                    {
                        sharedPropertyBlocks.Remove(instanceId);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize this instance and its property block
        /// </summary>
        private void InitializeInstance()
        {
            // Get or create the shared property block for this ID
            if (!sharedPropertyBlocks.ContainsKey(instanceId))
            {
                sharedPropertyBlocks[instanceId] = new MaterialPropertyBlock();
            }
            
            propertyBlock = sharedPropertyBlocks[instanceId];
            
            // Initialize property values if needed
            if (initializeFromMaterial && targetRenderer.sharedMaterial != null)
            {
                InitializePropertiesFromMaterial();
            }
        }

        /// <summary>
        /// Initialize property values from the material
        /// </summary>
        private void InitializePropertiesFromMaterial()
        {
            Material mat = targetRenderer.sharedMaterial;
            
            foreach (var prop in controlledProperties)
            {
                // Only initialize if the material has the property
                if (!mat.HasProperty(prop.name)) continue;
                
                switch (prop.type)
                {
                    case PropertyDefinition.PropertyType.Float:
                        prop.floatValue = mat.GetFloat(prop.name);
                        SetFloat(prop.name, prop.floatValue, false);
                        break;
                    case PropertyDefinition.PropertyType.Color:
                        prop.colorValue = mat.GetColor(prop.name);
                        SetColor(prop.name, prop.colorValue, false);
                        break;
                    case PropertyDefinition.PropertyType.Vector4:
                        prop.vectorValue = mat.GetVector(prop.name);
                        SetVector(prop.name, prop.vectorValue, false);
                        break;
                    case PropertyDefinition.PropertyType.Integer:
                        prop.intValue = mat.GetInt(prop.name);
                        SetInt(prop.name, prop.intValue, false);
                        break;
                    case PropertyDefinition.PropertyType.Texture:
                        prop.textureValue = mat.GetTexture(prop.name);
                        SetTexture(prop.name, prop.textureValue, false);
                        break;
                }
            }
            
            ApplyPropertyBlock();
        }

        /// <summary>
        /// Apply the current property block to the renderer
        /// </summary>
        private void ApplyPropertyBlock()
        {
            if (targetRenderer != null)
            {
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        /// <summary>
        /// Apply the property block to all renderers in the same ID group
        /// </summary>
        private static void ApplyPropertyBlockToGroup(string id)
        {
            if (!instanceGroups.ContainsKey(id)) return;
            
            foreach (var instance in instanceGroups[id])
            {
                if (instance != null && instance.enabled && instance.gameObject.activeInHierarchy)
                {
                    instance.ApplyPropertyBlock();
                }
            }
        }

        #region Property Setters
        /// <summary>
        /// Set a float property value
        /// </summary>
        public void SetFloat(string propertyName, float value, bool applyImmediately = true)
        {
            propertyBlock.SetFloat(propertyName, value);
            if (applyImmediately) ApplyPropertyBlockToGroup(instanceId);
            
            // Update the serialized value to keep inspector in sync
            for (int i = 0; i < controlledProperties.Count; i++)
            {
                if (controlledProperties[i].name == propertyName && 
                    controlledProperties[i].type == PropertyDefinition.PropertyType.Float)
                {
                    controlledProperties[i].floatValue = value;
                    break;
                }
            }
        }

        /// <summary>
        /// Set a color property value
        /// </summary>
        public void SetColor(string propertyName, Color value, bool applyImmediately = true)
        {
            propertyBlock.SetColor(propertyName, value);
            if (applyImmediately) ApplyPropertyBlockToGroup(instanceId);
            
            // Update the serialized value
            for (int i = 0; i < controlledProperties.Count; i++)
            {
                if (controlledProperties[i].name == propertyName && 
                    controlledProperties[i].type == PropertyDefinition.PropertyType.Color)
                {
                    controlledProperties[i].colorValue = value;
                    break;
                }
            }
        }

        /// <summary>
        /// Set a vector property value
        /// </summary>
        public void SetVector(string propertyName, Vector4 value, bool applyImmediately = true)
        {
            propertyBlock.SetVector(propertyName, value);
            if (applyImmediately) ApplyPropertyBlockToGroup(instanceId);
            
            // Update the serialized value
            for (int i = 0; i < controlledProperties.Count; i++)
            {
                if (controlledProperties[i].name == propertyName && 
                    controlledProperties[i].type == PropertyDefinition.PropertyType.Vector4)
                {
                    controlledProperties[i].vectorValue = value;
                    break;
                }
            }
        }

        /// <summary>
        /// Set an integer property value
        /// </summary>
        public void SetInt(string propertyName, int value, bool applyImmediately = true)
        {
            propertyBlock.SetInt(propertyName, value);
            if (applyImmediately) ApplyPropertyBlockToGroup(instanceId);
            
            // Update the serialized value
            for (int i = 0; i < controlledProperties.Count; i++)
            {
                if (controlledProperties[i].name == propertyName && 
                    controlledProperties[i].type == PropertyDefinition.PropertyType.Integer)
                {
                    controlledProperties[i].intValue = value;
                    break;
                }
            }
        }

        /// <summary>
        /// Set a texture property value
        /// </summary>
        public void SetTexture(string propertyName, Texture value, bool applyImmediately = true)
        {
            propertyBlock.SetTexture(propertyName, value);
            if (applyImmediately) ApplyPropertyBlockToGroup(instanceId);
            
            // Update the serialized value
            for (int i = 0; i < controlledProperties.Count; i++)
            {
                if (controlledProperties[i].name == propertyName && 
                    controlledProperties[i].type == PropertyDefinition.PropertyType.Texture)
                {
                    controlledProperties[i].textureValue = value;
                    break;
                }
            }
        }
        #endregion

        /// <summary>
        /// Change the instance ID at runtime
        /// </summary>
        public void ChangeInstanceId(string newId)
        {
            if (newId == instanceId) return;
            
            // Remove from current group
            if (instanceGroups.ContainsKey(instanceId))
            {
                instanceGroups[instanceId].Remove(this);
            }
            
            // Update the ID
            instanceId = newId;
            
            // Initialize with new ID
            InitializeInstance();
            
            // Register with new group
            if (!instanceGroups.ContainsKey(instanceId))
            {
                instanceGroups[instanceId] = new HashSet<MaterialPropertyInstance>();
            }
            instanceGroups[instanceId].Add(this);
            
            // Apply the property block
            ApplyPropertyBlock();
        }

        /// <summary>
        /// Add a property definition at runtime
        /// </summary>
        public void AddControlledProperty(string name, PropertyDefinition.PropertyType type)
        {
            // Check if it already exists
            foreach (var prop in controlledProperties)
            {
                if (prop.name == name && prop.type == type)
                {
                    return; // Already exists
                }
            }
            
            // Add new property
            PropertyDefinition newProp = new PropertyDefinition
            {
                name = name,
                type = type
            };
            
            controlledProperties.Add(newProp);
            
            // Initialize from material if needed
            if (initializeFromMaterial && targetRenderer.sharedMaterial != null)
            {
                Material mat = targetRenderer.sharedMaterial;
                if (mat.HasProperty(name))
                {
                    switch (type)
                    {
                        case PropertyDefinition.PropertyType.Float:
                            newProp.floatValue = mat.GetFloat(name);
                            SetFloat(name, newProp.floatValue);
                            break;
                        case PropertyDefinition.PropertyType.Color:
                            newProp.colorValue = mat.GetColor(name);
                            SetColor(name, newProp.colorValue);
                            break;
                        case PropertyDefinition.PropertyType.Vector4:
                            newProp.vectorValue = mat.GetVector(name);
                            SetVector(name, newProp.vectorValue);
                            break;
                        case PropertyDefinition.PropertyType.Integer:
                            newProp.intValue = mat.GetInt(name);
                            SetInt(name, newProp.intValue);
                            break;
                        case PropertyDefinition.PropertyType.Texture:
                            newProp.textureValue = mat.GetTexture(name);
                            SetTexture(name, newProp.textureValue);
                            break;
                    }
                }
            }
        }
    }
}