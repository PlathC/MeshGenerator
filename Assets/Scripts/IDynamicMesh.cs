using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IDynamicMesh {

    /// <summary>
    /// Generate a dynamic mesh
    /// </summary>
    /// <returns>The created mesh</returns>
    Mesh GenerateMesh();
}
