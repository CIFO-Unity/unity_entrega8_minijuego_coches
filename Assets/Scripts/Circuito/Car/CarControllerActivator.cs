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
        if (carControllers == null || carControllers.Length == 0) return;
        foreach (var ctrl in carControllers)
        {
            if (ctrl != null)
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
