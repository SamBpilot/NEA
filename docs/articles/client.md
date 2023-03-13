# Installing The Dummy Client

> [!TIP]
> You should install git.

Developers are able to create their own client apps to connect to the NEA server. This allows for a more customised experience for the user. The client app is a dummy app that is used to connect to the NEA server. The client app is not required to be installed on the device, but it is recommended. For more information, please read [Custom Clients](custom.md).

## Installing the dummy client

1) navigate to the [releases page](https://github.com/wotanut/NEA/releases) and download the latest release marked `client.ipa`
2) Sideload the app using [AltStore](https://altstore.io/) or [Sideloadly](https://sideloadly.io)

# [Installing through AltStore](#tab/altstore)

Installing through Altstore is simple. Just follow the below steps:

1) Navigate to https://altstore.io/ and download AltServer
2) Connect your iPhone, iPad or iPod to your computer
3) Launch AltSerber and click `Install AltStore`
4) Click on your device
5) Launch Altstore
6) Navigate to your device's settings and trust the AltStore profile. (General > VPN & Device Management))
7) Move the release you downloaded above to a folder that is shared with your device, this can typically be done by moving the ipa to your iCloud drive folder
8) Open Altstore and click the `+` button in the top right
9) Find the ipa in the folder you moved it to and click `Open`
10) Click `Install` and wait for the app to install
11) Enjoy the app :)

> [!NOTE]
> If you are using a free Apple Developer account, you will need to re-install the app every 7 days. This is due to the fact that the certificate will expire after 7 days.

# [Installing through Sideloadly](#tab/sideloadly)

Installing through Sideloadly is simple. Just follow the below steps:

1) Navigate to https://sideloadly.io/ and download Sideloadly
2) Connect your iPhone, iPad or iPod to your computer
3) Launch Sideloadly and drag the ipa onto the window
4) Select your device and apple ID and click `Install`
5) Enjoy the app :)

> [!NOTE]
> If you are using a free Apple Developer account, you will need to re-install the app every 7 days. This is due to the fact that the certificate will expire after 7 days.



## Building the dummy client

To install development builds, you __must__ have git installed. To clone the repository run:

``` git clone https://github.com/wotanut/NEA.git ```

Then open the Client folder in xCode and run the app.