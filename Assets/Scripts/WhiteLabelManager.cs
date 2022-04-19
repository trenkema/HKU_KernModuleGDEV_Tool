using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class WhiteLabelManager : MonoBehaviour
{
    [SerializeField] GameObject authenticationUI;
    [SerializeField] GameObject mainMenuUI;

    // New User
    [SerializeField] TMP_InputField newEmailInputField;
    [SerializeField] TMP_InputField newUsernameInputField;
    [SerializeField] TMP_InputField newPasswordInputField;

    // Existing User
    [SerializeField] TMP_InputField existingEmailInputField;
    [SerializeField] TMP_InputField existingUsernameInputField;
    [SerializeField] TMP_InputField existingPasswordInputField;

    [SerializeField] TextMeshProUGUI errorText;

    public void Login()
    {
        errorText.gameObject.SetActive(false);

        string email = existingEmailInputField.text;
        string password = existingPasswordInputField.text;

        LootLockerSDKManager.WhiteLabelLogin(email, password, false, response =>
        {
            if (!response.success)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Check login info";

                Debug.Log("Error Logging In");
                return;
            }

            Debug.Log("Player Logged In");

            LootLockerSDKManager.StartWhiteLabelSession((response) =>
            {
                if (!response.success)
                {
                    Debug.Log("Error Starting Session");
                    return;
                }
                else
                {
                    authenticationUI.SetActive(false);
                    mainMenuUI.SetActive(true);
                }
            });
        });
    }

    public void NewUser()
    {
        errorText.gameObject.SetActive(false);

        string email = newEmailInputField.text;
        string password = newPasswordInputField.text;
        string username = newUsernameInputField.text;

        LootLockerSDKManager.WhiteLabelSignUp(email, password, (response) =>
        {
            if (!response.success)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Check create info";

                Error(response.Error);
                return;
            }
            else
            {
                LootLockerSDKManager.WhiteLabelLogin(email, password, false, response =>
                {
                    if (!response.success)
                    {
                        Error(response.Error);
                        return;
                    }

                    LootLockerSDKManager.StartWhiteLabelSession((response) =>
                    {
                        if (!response.success)
                        {
                            Error(response.Error);
                            return;
                        }

                        if (username == string.Empty)
                        {
                            username = response.public_uid;
                        }

                        LootLockerSDKManager.SetPlayerName(username, (response) =>
                        {
                            if (!response.success)
                            {
                                Error(response.Error);
                                return;
                            }

                            LootLockerSessionRequest sessionRequest = new LootLockerSessionRequest();

                            LootLocker.LootLockerAPIManager.EndSession(sessionRequest, (response) =>
                            {
                                if (!response.success)
                                {
                                    Error(response.Error);
                                    return;
                                }

                                Debug.Log("Account Created");
                            });
                        });
                    });
                });
            }
        });
    }

    private void Error(string _error)
    {
        Debug.Log("Error: " + _error);
    }
}
