﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class AFrameExporter : ScriptableObject {

    [HeaderAttribute("General")]
    public string title = "Hello world!";
    public string libraryAddress = "https://aframe.io/releases/latest/aframe.min.js";
    public bool enable_performance_statistics = false;
    [HeaderAttribute("Sky")]
    public bool enable_sky = false;
    public bool sky_color_from_MainCamera_Background = true;
    public Color sky_color = Color.white;
    public Texture sky_texture;
    [HeaderAttribute("Camera")]
    public bool wasd_controls_enabled = true;
    public bool look_controls_enabled = true;
    public bool cursor_visible = true;
    public float cursor_opacity = 1f;
    public float cursor_scale = 1f;
    public Color cursor_color = Color.white;
    public float cursor_offset = 1f;
    public float cursor_maxdistance = 1000f;

    private string indent = "      ";
    private string exporter_path = "Assets/AFrameExporter";
    private string export_path = "Assets/AFrameExporter/export";
    private string export_filename = "index.html";

    //A-FrameをExportする
    public void Export()
    {
        TextAsset template_head = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter_path + "/template_head.txt");
        TextAsset template_append = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter_path + "/template_append.txt");
        TextAsset template_end = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter_path + "/template_end.txt");

        //exportフォルダが無ければ作る
        string guid_exist = AssetDatabase.AssetPathToGUID(export_path);
        if (!File.Exists(Application.dataPath + "/AframeExporter/export"))
        {
            AssetDatabase.CreateFolder("Assets/AFrameExporter", "export");
            AssetDatabase.Refresh();
        }

        //imagesフォルダ作る
        guid_exist = AssetDatabase.AssetPathToGUID(export_path + "/images");
        if (!File.Exists(Application.dataPath + "/AframeExporter/export/images"))
        {
            AssetDatabase.CreateFolder(export_path, "images");
            AssetDatabase.Refresh();
        }

        //modelsフォルダ作る
        guid_exist = AssetDatabase.AssetPathToGUID(export_path + "/models");
        if (!File.Exists(Application.dataPath + "/AframeExporter/models"))
        {
            AssetDatabase.CreateFolder(export_path, "models");
            AssetDatabase.Refresh();
        }

        //シーンをコンバート
        string scene_string = convertScene();

        string body_string = template_head.text + scene_string + template_append.text + template_end.text;

        //タイトルなど差し替え
        body_string = body_string.Replace("&TITLE&", title);
        body_string = body_string.Replace("&LIBRARY&", libraryAddress);
        if (enable_performance_statistics)
        {
            body_string = body_string.Replace("a-scene","a-scene stats=\"true\"");
        }

        File.WriteAllText(Application.dataPath + "/AFrameExporter/export/" + export_filename, body_string);
        AssetDatabase.Refresh();
    }

    private string convertScene()
    {
        string ret_str = "";
        bool isThereCamera = false;
        bool isThereLight = false;

        //sky
        if (enable_sky)
        {
            string add_str = "<a-sky ";

            Camera mainCamera = Camera.main;
            if (sky_color_from_MainCamera_Background && mainCamera)
            {
                add_str += "color=\"#" + ColorToHex(mainCamera.backgroundColor) + "\" ";
            }
            else
            {
                add_str += "color=\"#" + ColorToHex(sky_color) + "\" ";
            }

            if (sky_texture)
            {
                string texture_path = AssetDatabase.GetAssetPath(sky_texture);
                string new_path = export_path + "/images/" + Path.GetFileName(texture_path);
                //テクスチャ無ければコピー
                if (AssetDatabase.AssetPathToGUID(new_path) == "")
                {
                    AssetDatabase.CopyAsset(texture_path, new_path);
                }

                add_str += "src=\"images/" + Path.GetFileName(texture_path) + "\" ";
            }

            add_str += "></a-sky>\n";
            ret_str += add_str;
        }

        //オブジェクト全検索
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            // ProjectにあるものならAsset Path, SceneにあるオブジェクトはそのシーンのPathが返ってくる
            string path = AssetDatabase.GetAssetOrScenePath(obj);

            // シーンのPathは拡張子が .unity
            string sceneExtension = ".unity";

            // Path.GetExtension(path) で pathの拡張子を取得
            // Equals(sceneExtension)で sceneExtensionと比較
            bool isExistInScene = Path.GetExtension(path).Equals(sceneExtension);

            // シーン内のオブジェクトかどうか判定
            if (isExistInScene)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                Light light = obj.GetComponent<Light>();
                //メッシュフィルターコンポーネントを持ってるなら
                if (meshFilter && meshFilter.sharedMesh)
                {
                    //Cubeの場合
                    if (meshFilter.sharedMesh.name == "Cube")
                    {
                        Vector3 scale = obj.transform.lossyScale;

                        string append_str = indent + "<a-entity geometry=\"primitive: box; width: " + scale.x + "; height: " + scale.y + "; depth: " + scale.z + "\" " + outputRotation(obj) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                        ret_str += append_str;
                    }
                    //TODO:Sphereの場合
                    if (meshFilter.sharedMesh.name == "Sphere")
                    {
                        //各軸scaleが使えないので各スケールの平均値をradiusとする
                        Vector3 scale = obj.transform.lossyScale;
                        float radius = 0.5f;
                        if (scale != Vector3.one)
                        {
                            radius = (scale.x + scale.y + scale.z) * 0.333333333f * 0.5f;
                        }
                        string append_str = indent + "<a-entity geometry=\"primitive: sphere; radius: " + radius + "\" " + outputRotation(obj) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                        ret_str += append_str;
                    }
                    //TODO:Cylinderの場合
                    //TODO:Planeの場合
                    //TODO:Modelの場合
                }
                //MainCameraの場合
                else if (!isThereCamera && obj.tag == "MainCamera")
                {
                    Camera camera = obj.GetComponent<Camera>();
                    if (camera)
                    {
                        string append_str = indent + "<a-camera " + outputPosition(obj) + " cursor-color=#" + ColorToHex(cursor_color) +
                            " wasd-controls-enabled=" + wasd_controls_enabled.ToString().ToLower() + " fov=" + camera.fieldOfView + " near=" + camera.nearClipPlane + " far=" + camera.farClipPlane +
                            " cursor-maxdistance=" + cursor_maxdistance + " cursor-offset=" + cursor_offset + " cursor-opacity=" + cursor_opacity + " cursor-scale" + cursor_scale +
                            " look-controls-enabled=" + look_controls_enabled.ToString().ToLower() + "></a-camera>\n";
                        ret_str += append_str;
                        isThereCamera = true;
                    }
                }
                //Lightの場合
                else if (light)
                {
                    //DirectionalLightの場合
                    if (light.type == LightType.Directional)
                    {
                        Vector3 forward = -obj.transform.forward;
                        string lightPosition_str = "position=\"" + -forward.x + " " + forward.y + " " + forward.z + "\" ";
                        string append_str = indent + "<a-light type=directional; intensity=" + light.intensity + " color=#" + ColorToHex(light.color) + " "  + lightPosition_str + "></a-light>\n";
                        ret_str += append_str;
                        isThereLight = true;
                    }
                    //TODO:PointLightの場合
                    //TODO:SpotLightの場合
                    //TODO:AmbientLightの場合
                    //TODO:Hemisphereの場合
                }
            }
        }

        //Cameraが無い場合はデフォルト設定
        if (!isThereCamera)
        {
            string append_str = indent + "<a-camera cursor-color=#" + ColorToHex(cursor_color) +
                            " wasd-controls-enabled=" + wasd_controls_enabled.ToString().ToLower() + "></a-camera>\n";
            ret_str += append_str;
            ret_str += append_str;
        }

        //Lightが無い場合はデフォルトライト
        if (!isThereLight)
        {
        }

        return ret_str;
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    private string outputRotation(GameObject obj)
    {
        Vector3 rotation = obj.transform.rotation.eulerAngles;
        if (rotation == Vector3.zero)
        {
            return "";
        }
        return "rotation=\"" + rotation.x + " " + -rotation.y + " " + -rotation.z + "\" ";
    }

    private string outputMaterial(GameObject obj)
    {
        string ret_str = "";

        Material mat = obj.GetComponent<Renderer>().sharedMaterial;
        if (!mat)
        {
            return ret_str;
        }

        //Debug.Log(mat.shader.name);
        //シェーダはこれらだけサポート
        if (mat.shader.name == "Standard")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: standard; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //カラー
            ret_str += "color: #" + ColorToHex(mat.color) + "; ";

            //メタルネス
            ret_str += "metalness: " + mat.GetFloat("_Metallic") + "; ";

            //スムースネス（roughnessの逆)
            ret_str += "roughness: " + (1f-mat.GetFloat("_Glossiness")) + "; ";

            //透過有効(_Modeが３ならRendering Modeはtransparent)
            ret_str += "transparent: " + (mat.GetFloat("_Mode") == 3 ? "true" : "false") + "; ";

            //透明度
            ret_str += "opacity: " + mat.color.a + "; ";

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Unlit/Color")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Unlit/Texture")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Unlit/Texture Colored")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //カラー
            ret_str += "color: #" + ColorToHex(mat.color) + "; ";

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Legacy Shaders/Transparent/Diffuse")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //カラー
            ret_str += "color: #" + ColorToHex(mat.color) + "; ";

            //透過有効(_Modeが３ならRendering Modeはtransparent)
            ret_str += "transparent: true; ";

            //透明度
            ret_str += "opacity: " + mat.color.a + "; ";

            //おしまい
            ret_str += "\"";
        }

        return ret_str;
    }

    private string outputTexture(Material mat)
    {
        //テクスチャ
        Texture tex = mat.GetTexture("_MainTex");
        if (tex)
        {
            string texture_path = AssetDatabase.GetAssetPath(tex);
            string new_path = export_path + "/images/" + Path.GetFileName(texture_path);
            //テクスチャ無ければコピー
            if (AssetDatabase.AssetPathToGUID(new_path) == "")
            {
                AssetDatabase.CopyAsset(texture_path, new_path);
            }

            return "src: url(images/" + Path.GetFileName(texture_path) + "); ";
        }
        return "";
    }

    private string outputPosition(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        if (position == Vector3.zero)
        {
            return "";
        }
        //return "translate=\"" + -position.x + " " + position.y + " " + position.z + "\" ";
        return "position=\"" + -position.x + " " + position.y + " " + position.z + "\" ";
    }

    //A-Frameを実行する
    public void RunAFrame()
    {
        System.Diagnostics.Process.Start(Application.dataPath + "/AFrameExporter/export/" + export_filename);
    }

    //エクスポートしたA-Frameをクリア
    public void CrearExport()
    {
        string guid_exist = AssetDatabase.AssetPathToGUID(export_path);
        if (guid_exist != "")
        {
            Directory.Delete(Application.dataPath + "/AFrameExporter/export/", true);
        }
    }
}