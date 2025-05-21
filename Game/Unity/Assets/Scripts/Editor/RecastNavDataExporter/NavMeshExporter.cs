// using System;
// using System.IO;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using ET;
// using Unity.AI.Navigation;
// using UnityEditor;
// using UnityEditor.UI;
// using UnityEngine;
// using UnityEngine.AI;
// using UnityEngine.SceneManagement;
//
// namespace ETEditor
// {
//     /// <summary>
//     /// 从Unity的NavMesh组件里导出地图数据，供服务器来使用
//     /// https://blog.csdn.net/huutu/article/details/52672505
//     /// </summary>
//     public class NavMeshExporter: Editor
//     {
//         private static string outputClientFolder = "../Tools/RecastNavExportor/Meshes/";
//         private static string outputServerFolder = "../Config/RecastNavData/ExportedObj/";
//
//         [MenuItem("ET/NavMesh/ExportSceneObj")]
//         public static void ExportScene()
//         {
//             var triangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();
//             if (triangulation.indices.Length < 3)
//             {
//                 Debug.LogError($"NavMeshExporter ExportScene Error - 场景里没有需要被导出的物体，请先用NavMesh进行Bake。");
//                 return;
//             }
//
//             var activeGameObject = Selection.activeGameObject;
//             if (activeGameObject == null || activeGameObject.GetComponent<NavMeshSurface>() == null)
//             {
//                 Debug.LogError($"请选中一个挂了NavMeshSurface的组件的物体");
//                 return; 
//             }
//
//           
//
//             NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
//
//             if (!Directory.Exists(outputClientFolder))
//             {
//                 Directory.CreateDirectory(outputClientFolder);//不存在就创建目录
//             }
//             //文件路径
//             string path = outputClientFolder + activeGameObject.name + ".obj";
//             Debug.Log($"NavMesh Export Start 导出的寻路文件名为 {activeGameObject.name }");
//             //新建文件
//             StreamWriter streamWriter = new StreamWriter(path);
//
//             //顶点  
//             for (int i = 0; i < navMeshTriangulation.vertices.Length; i++)
//             {
//                 streamWriter.WriteLine("v  " + (-1 * navMeshTriangulation.vertices[i].x) + " " + navMeshTriangulation.vertices[i].y + " " + navMeshTriangulation.vertices[i].z);
//             }
//
//             streamWriter.WriteLine("g pPlane1");
//
//             //索引  
//             for (int i = 0; i < navMeshTriangulation.indices.Length;)
//             {
//                 streamWriter.WriteLine("f " + (navMeshTriangulation.indices[i] + 1) + " " + (navMeshTriangulation.indices[i + 1] + 2) + " " + (navMeshTriangulation.indices[i + 1] + 1));
//                 i = i + 3;
//             }
//
//             streamWriter.Flush();
//             streamWriter.Close();
//
//
//             AssetDatabase.Refresh();
//
//             Debug.Log("NavMesh Export Success");
//         }
//     }
// }