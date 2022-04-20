using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class WhiteLabelManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI loggedInNameText;

    [SerializeField] GameObject authenticationUI;
    [SerializeField] GameObject loginUI;
    [SerializeField] GameObject registerUI;

    [SerializeField] GameObject mainMenuUI;

    // New User
    [SerializeField] TMP_InputField newEmailInputField;
    [SerializeField] TMP_InputField newUsernameInputField;
    [SerializeField] TMP_InputField newPasswordInputField;

    // Existing User
    [SerializeField] TMP_InputField existingEmailInputField;
    [SerializeField] TMP_InputField existingPasswordInputField;

    [SerializeField] TextMeshProUGUI errorText;

    string playerName = "";

    private void Start()
    {
        loggedInNameText.gameObject.SetActive(false);
        authenticationUI.SetActive(false);
        mainMenuUI.SetActive(false);

        CheckForPreviousSession();
    }

    private void CheckForPreviousSession()
    {
        LootLockerSDKManager.CheckWhiteLabelSession(response =>
        {
            if (!response)
            {
                authenticationUI.SetActive(true);
            }
            else
            {
                LootLockerSDKManager.StartWhiteLabelSession((response) =>
                {
                    if (!response.success)
                    {
                        authenticationUI.SetActive(true);

                        Debug.Log("Error Starting Session");
                        return;
                    }
                    else
                    {
                        LootLockerSDKManager.GetPlayerName(response =>
                        {
                            playerName = response.name;

                            loggedInNameText.gameObject.SetActive(true);

                            loggedInNameText.text = "LOGGED IN: " + playerName;
                        });

                        authenticationUI.SetActive(false);
                        mainMenuUI.SetActive(true);
                    }
                });
            }
        });
    }

    public void Logout()
    {
        LootLockerSessionRequest sessionRequest = new LootLockerSessionRequest();

        LootLocker.LootLockerAPIManager.EndSession(sessionRequest, (response) =>
        {
            if (!response.success)
            {
                Error(response.Error);
                return;
            }

            loggedInNameText.gameObject.SetActive(false);
            mainMenuUI.SetActive(false);
            authenticationUI.SetActive(true);
            registerUI.SetActive(false);
            loginUI.SetActive(true);

            Debug.Log("Account Created");
        });
    }

    public void Login()
    {
        errorText.gameObject.SetActive(false);

        string email = existingEmailInputField.text;
        string password = existingPasswordInputField.text;

        LootLockerSDKManager.WhiteLabelLogin(email, password, true, response =>
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
                    LootLockerSDKManager.GetPlayerName(response =>
                    {
                        playerName = response.name;

                        loggedInNameText.gameObject.SetActive(true);

                        loggedInNameText.text = "LOGGED IN: " + playerName;
                    });

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
                LootLockerSDKManager.WhiteLabelLogin(email, password, true, response =>
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

                                registerUI.SetActive(false);
                                loginUI.SetActive(true);

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
