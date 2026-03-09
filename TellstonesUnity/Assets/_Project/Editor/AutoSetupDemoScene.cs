using UnityEngine;
using UnityEditor;
using Tellstones.Core;
using Tellstones.Visual;
using Tellstones.AI.Personalities;
using Tellstones.InputSystem;
using Tellstones.AI;

namespace Tellstones.Editor
{
    public class AutoSetupDemoScene
    {
        [MenuItem("Tellstones/1. Gerar Cena Base de Testes")]
        public static void GenerateScene()
        {
            Debug.Log("Iniciando geração da Cena de Demostração...");

            // 1. Câmera
            var camObj = Camera.main;
            if (camObj == null)
            {
                camObj = new GameObject("Main Camera").AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            camObj.transform.position = new Vector3(0, 8, -5);
            camObj.transform.rotation = Quaternion.Euler(60, 0, 0);
            
            if (camObj.GetComponent<CameraRig>() == null)
            {
                camObj.gameObject.AddComponent<CameraRig>();
            }
            if (camObj.GetComponent<PhysicsRaycaster>() == null) 
            {
                camObj.gameObject.AddComponent<PhysicsRaycaster>();
            }

            // Luz
            var lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = new GameObject("Directional Light");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
            }
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 2. Chão / Mesa (Visual Estético)
            var mesaObj = GameObject.Find("Mesa");
            if (mesaObj == null)
            {
                mesaObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesaObj.name = "Mesa";
                mesaObj.transform.localScale = new Vector3(8, 0.5f, 4);
                mesaObj.transform.position = new Vector3(0, -0.25f, 0);
            }

            // 3. Tabuleiro e Slots Lógicos (BoardView)
            var boardObj = GameObject.Find("Board");
            if (boardObj == null)
            {
                boardObj = new GameObject("Board");
            }
            
            var boardView = boardObj.GetComponent<BoardView>();
            if (boardView == null)
            {
                boardView = boardObj.AddComponent<BoardView>();
            }

            // Criação de 7 slots invisíveis (apenas para position transform e clique do Raycast)
            Transform[] slots = new Transform[7];
            float startX = -3f;
            float spacingX = 1f;

            for (int i = 0; i < 7; i++)
            {
                var slotObj = GameObject.Find($"Slot{i}");
                if (slotObj == null)
                {
                    // Um quad invisivel na mesa pra servir de colisor de click pro Raycast
                    slotObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    slotObj.name = $"Slot{i}";
                    slotObj.transform.SetParent(boardObj.transform);
                    slotObj.transform.position = new Vector3(startX + (i * spacingX), 0.1f, 0);
                    slotObj.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
                    
                    var col = slotObj.GetComponent<Collider>();
                    if (col != null) slotObj.layer = LayerMask.NameToLayer("Default"); // TODO: mudar layer

                    var renderer = slotObj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        var mat = new Material(Shader.Find("Standard"));
                        mat.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Semi Transparente cinza
                        renderer.material = mat;
                    }
                }
                slots[i] = slotObj.transform;
            }

            // Reflete no script do Inspector
            var soBoard = new SerializedObject(boardView);
            var slotsProp = soBoard.FindProperty("slotPositions");
            slotsProp.arraySize = 7;
            for (int i = 0; i < 7; i++)
            {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            }
            soBoard.ApplyModifiedProperties();

            // Prefab da Pedra PROVISÓRIA (Apenas pro Inspector não bugar nulo)
            // Na Unity real salva-se um prefab na pasta, criaremos um "Dummy" aqui e apagaremos da cena
            GameObject fakePrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fakePrefab.name = "DummyStonePrefab";
            fakePrefab.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            var stoneView = fakePrefab.AddComponent<StoneView>();
            
            soBoard.Update();
            soBoard.FindProperty("stonePrefab").objectReferenceValue = fakePrefab;
            soBoard.ApplyModifiedProperties();

            // 4. Managers
            var managerObj = GameObject.Find("GameManager");
            if (managerObj == null)
            {
                managerObj = new GameObject("GameManager");
            }

            var matchManager = managerObj.GetComponent<MatchManager>();
            if (matchManager == null) matchManager = managerObj.AddComponent<MatchManager>();

            var interaction = managerObj.GetComponent<InteractionManager>();
            if (interaction == null) interaction = managerObj.AddComponent<InteractionManager>();

            // Cria um ScriptableObject de teste e liga nele
            var profile = ScriptableObject.CreateInstance<MaestroProfile>();
            profile.profileName = "Bot Editor (Teste)";
            AssetDatabase.CreateAsset(profile, "Assets/_Project/AI/Personalities/DebugProfile.asset");
            AssetDatabase.SaveAssets();

            var soMatch = new SerializedObject(matchManager);
            soMatch.FindProperty("botProfile").objectReferenceValue = profile;
            soMatch.ApplyModifiedProperties();

            Debug.Log("Cena gerada com sucesso! Um [DummyStonePrefab] foi criado na mesa. Arraste-o para a aba Project para criar um prefab DE VERDADE e conecte no script BoardView do objeto 'Board', e apague da tela.");
        }
    }
}
