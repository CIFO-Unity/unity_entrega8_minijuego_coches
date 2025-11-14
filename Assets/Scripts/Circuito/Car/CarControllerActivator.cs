using UnityEngine;

public class CarControllerActivator : MonoBehaviour
{
    [Header("Car Controllers")]
    // Controllers will be auto-detected at Start; no inspector assignment required.
    private PrometeoCarController[] carControllers;

    private void Start()
    {
        // If nothing is assigned in the inspector, try to auto-detect controllers in children or the scene.
        if (carControllers == null || carControllers.Length == 0)
        {
            // Prefer children of this GameObject
            var foundInChildren = GetComponentsInChildren<PrometeoCarController>(true);
            if (foundInChildren != null && foundInChildren.Length > 0)
            {
                carControllers = foundInChildren;
            }
            else
            {
                // Fallback: find all controllers in the scene
                var foundInScene = FindObjectsOfType<PrometeoCarController>(true);
                if (foundInScene != null && foundInScene.Length > 0)
                    carControllers = foundInScene;
            }
        }

        if (carControllers != null && carControllers.Length > 0)
        {
            // Disable all assigned controllers at start
            foreach (var ctrl in carControllers)
            {
                if (ctrl != null)
                    ctrl.enabled = false;
            }
        }
        else
        {
            // No controllers found or assigned; nothing to disable.
        }
    }

    /// <summary>
    /// Activa el control de todos los vehículos asignados.
    /// </summary>
    public void ActivateCarControl()
    {
        // At activation time, detect current controllers in the scene to ensure we
        // enable controllers that may have been created or assigned after Start().
        // Use Resources.FindObjectsOfTypeAll to include inactive scene objects across Unity versions,
        // then filter to instances belonging to loaded scenes (exclude assets/prefabs).
        var all = Resources.FindObjectsOfTypeAll<PrometeoCarController>();
        if (all == null || all.Length == 0) return;
        foreach (var ctrl in all)
        {
            if (ctrl == null) continue;
            // Ensure this instance belongs to a loaded scene (not a prefab asset)
            if (!ctrl.gameObject.scene.IsValid() || !ctrl.gameObject.scene.isLoaded) continue;
            ctrl.enabled = true;
        }
    }

    /// <summary>
    /// Desactiva el control de todos los vehículos asignados.
    /// </summary>
    public void DeactivateCarControl()
    {
        if (carControllers == null) return;
        foreach (var ctrl in carControllers)
        {
            if (ctrl != null)
                ctrl.enabled = false;
        }
    }
}
