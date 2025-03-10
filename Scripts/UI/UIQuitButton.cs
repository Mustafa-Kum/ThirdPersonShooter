using UnityEngine;
using UnityEngine.EventSystems;

namespace UILogic
{
    /// <summary>
    /// Handles application exit functionality with a button.
    /// Follows Single Responsibility Principle by focusing only on quit functionality.
    /// </summary>
    public class UIQuitButton : UIButton
    {
        [SerializeField] private bool _showConfirmationDialog = false;
        [SerializeField] private string _confirmationMessage = "Do you really want to quit?";
        
        /// <summary>
        /// Called when pointer is clicked on the button.
        /// Open for extension through the virtual methods (Open/Closed Principle).
        /// </summary>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            
            if (_showConfirmationDialog)
            {
                RequestQuitConfirmation();
            }
            else
            {
                ExecuteQuit();
            }
        }
        
        /// <summary>
        /// Shows a confirmation dialog before quitting.
        /// Can be overridden by derived classes to customize confirmation behavior.
        /// </summary>
        protected virtual void RequestQuitConfirmation()
        {
            // In a real implementation, this might show a UI dialog
            // For now, just log and quit directly
            Debug.Log($"Confirmation: {_confirmationMessage}");
            ExecuteQuit();
        }
        
        /// <summary>
        /// Executes the actual quit operation.
        /// Can be overridden by derived classes to customize quit behavior.
        /// </summary>
        protected virtual void ExecuteQuit()
        {
            Debug.Log("Quitting game...");
            
            #if UNITY_STANDALONE || UNITY_WEBGL
                QuitApplication();
            #elif UNITY_EDITOR
                QuitEditor();
            #elif UNITY_ANDROID || UNITY_IOS
                QuitMobileApplication();
            #endif
        }
        
        /// <summary>
        /// Quits the application in standalone builds.
        /// </summary>
        protected virtual void QuitApplication()
        {
            Application.Quit();
        }
        
        /// <summary>
        /// Quits play mode in the Unity Editor.
        /// </summary>
        protected virtual void QuitEditor()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        
        /// <summary>
        /// Quits the application on mobile platforms.
        /// Can implement platform-specific behavior if needed.
        /// </summary>
        protected virtual void QuitMobileApplication()
        {
            Application.Quit();
        }
    }
}

