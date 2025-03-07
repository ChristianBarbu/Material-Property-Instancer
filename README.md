# Unity Material Property Instancer

## Overview

**Unity Material Property Instancer** is a lightweight solution for controlling material properties on a per-instance basis while still using shared materials. It optimizes GPU memory usage while giving you granular control over shader properties. With this package, you can:

- **Control shader properties independently:** Adjust properties on individual objects without affecting others.
- **Group renderers:** Multiple renderers can share property values using a common Instance ID.
- **Runtime changes:** Easily update property values through a simple API.
- **Renderer support:** Works with all Unity renderer types (e.g., MeshRenderer, LineRenderer).
- **Property type support:** Compatible with all standard shader property types (float, color, vector, integer, texture).


## Installation

### Method 1: Unity Package Manager (UPM)
1. Open the **Package Manager** window (`Window > Package Manager`).
2. Click the **+** button in the top-left corner.
3. Select **"Add package from git URL..."**.
4. Enter the repository URL:
[https://github.com/yourusername/MaterialPropertyInstancer.git](https://github.com/ChristianBarbu/Material-Property-Instancer.git)

5. Click **"Add"**.

### Method 2: Manual Installation
1. Download the latest release from the repository.
2. Extract the package into your project's **Assets** folder.
3. The package will then be available in your project.

## Quick Start

1. **Add the Component:**  
Attach the `MaterialPropertyInstance` component to any GameObject with a Renderer.
2. **Configure Properties:**  
In the component, add the shader properties you want to control to the **Controlled Properties** list.
3. **Set Instance ID:**  
Assign a unique **Instance ID** for independent control, or use the same ID across objects to group them.
4. **Update at Runtime:**  
Use the provided API to modify property values during runtime.

## Core Components

### MaterialPropertyInstance
This is the main component for managing material properties. It supports both instance-specific and group-specific configurations for renderers.

#### Inspector Properties

- **Instance ID:**  
A unique identifier. Objects with the same ID will share property values.
- **Controlled Properties:**  
A list of shader properties that you want to adjust independently.
- **Initialize From Material:**  
When enabled, the component takes initial property values from the material itself.

### Property Definition Structure
Each controlled property includes:

- **Name:**  
The shader property name (e.g., `_FillAmount`).
- **Type:**  
The property type (Float, Color, Vector4, Integer, Texture).
- **Value:**  
The default value based on the chosen type.

## Usage Examples

### Basic Setup Example

![Preview Inspector](images/PreviewInspector.png)

```csharp
using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
 // Reference to the MaterialPropertyInstance component
 public MaterialPropertyInstance mpi;

 void Start()
 {
     // Set the '_FillAmount' property to 0.5 for this instance
     mpi.SetProperty("_FillAmount", 0.5f);
 }
}
