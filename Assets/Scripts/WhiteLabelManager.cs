using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class WhiteLabelManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI loggedInNameText;

    [SerializeField] GameObject logoutButton;

    [SerializeField] GameObject mainMenuTopText;

    [SerializeField] GameObject authenticationButtons;

    [SerializeField] GameObject authenticationUI;
    [SerializeField] GameObject loginUI;
    [SerializeField] GameObject registerUI;

    [SerializeField] GameObject selectModeUI;

    [SerializeField] GameObject mainMenuUI;

    // New User
    [SerializeField] TMP_InputField newEmailInputField;
    [SerializeField] TMP_InputField newUsernameInputField;
    [SerializeField] TMP_InputField newPasswordInputField;

    // Existing User
    [SerializeField] TMP_InputField existingEmailInputField;
    [SerializeField] TMP_InputField existingPasswordInputField;

    [SerializeField] TextMeshProUGUI loginErrorText;
    [SerializeField] TextMeshProUGUI registerErrorText;

    int playerDatabaseID = 2559;

    string playerName = "";

    private void Start()
    {
        logoutButton.SetActive(false);

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

                        logoutButton.SetActive(true);

                        authenticationUI.SetActive(false);
                        mainMenuUI.SetActive(true);

                        mainMenuTopText.SetActive(false);
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

            logoutButton.SetActive(false);

            loggedInNameText.gameObject.SetActive(false);
            selectModeUI.SetActive(false);
            mainMenuUI.SetActive(false);

            mainMenuTopText.SetActive(true);

            authenticationUI.SetActive(true);
            authenticationButtons.SetActive(true);

            registerUI.SetActive(false);
            loginUI.SetActive(false);

            Debug.Log("Account Created");
        });
    }

    public void Login()
    {
        loginErrorText.gameObject.SetActive(false);

        string email = existingEmailInputField.text;
        string password = existingPasswordInputField.text;

        LootLockerSDKManager.WhiteLabelLogin(email, password, true, response =>
        {
            if (!response.success)
            {
                loginErrorText.gameObject.SetActive(true);
                loginErrorText.text = "Check login info";

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

                    logoutButton.SetActive(true);

                    authenticationUI.SetActive(false);
                    mainMenuUI.SetActive(true);
                }
            });
        });
    }

    public void NewUser()
    {
        registerErrorText.gameObject.SetActive(false);

        string email = newEmailInputField.text;
        string password = newPasswordInputField.text;
        string username = newUsernameInputField.text;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Error(response.Error);

                return;
            }

            LootLockerSDKManager.GetScoreList(playerDatabaseID, 2000, (levelListResponse) =>
            {
                if (levelListResponse.statusCode == 200)
                {
                    for (int i = 0; i < levelListResponse.items.Length; i++)
                    {
                        if (levelListResponse.items[i].member_id == username)
                        {
                            registerErrorText.gameObject.SetActive(true);

                            registerErrorText.text = "User already exists";

                            return;
                        }
                    }

                    LootLockerSDKManager.SubmitScore(username, 0, playerDatabaseID, (scoreResponse) =>
                    {
                        if (scoreResponse.statusCode == 200)
                        {
                            LootLockerSessionRequest sessionRequest = new LootLockerSessionRequest();

                            LootLocker.LootLockerAPIManager.EndSession(sessionRequest, (response) =>
                            {
                                if (!response.success)
                                {
                                    Error(response.Error);
                                    return;
                                }

                                LootLockerSDKManager.WhiteLabelSignUp(email, password, (response) =>
                                {
                                    if (!response.success)
                                    {
                                        registerErrorText.gameObject.SetActive(true);
                                        registerErrorText.text = "Check create info";

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
                            });
                        }
                    });
                }
            });
        });
    }

    private void Error(string _error)
    {
        Debug.Log("Error: " + _error);
    }
}
