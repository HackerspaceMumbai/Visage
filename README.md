# Visage
The project is a checkin solution for our meetups using face recognition technology. 




### Solution Overview
![Solution Overview](./images/solution-overview.png)

The solution has 3 main components :
* FACE API : Microsoft Face API is used to register faces against attendees, and to identify attendees during check in
* Photo App : This NodeJS App is used by attendees to upload their photos for the event
* Checkin App : This UWP app is used for Participant checkin at the event venue

#### Key setup and configuration steps

##### Create Face API person group for event
* Person Group is the FACE API entity which logically corresponds to a event for which we want to checkin attendees. 
* First we need to get Face API Key from Azure Portal. Follow steps shown in [Get Face API key from Azure portal](http://www.c-sharpcorner.com/article/how-to-create-microsoft-cognitive-service-face-api-in-azure-portal/) to get your free or standard tier FACE API key.
* Now we create a person group for your event. The [FACE API reference](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395236) gives details of the RESTFUL FACE API. Code below shows how we can use curl to create a person group for our event. We need to substitute values for "yourmeetupgroupid" , "YourFaceAPISubscriptionKey" , "yourmeetupname" and "other-meetup-meta-data"

    ```sh
    curl -X PUT   https://westus.api.cognitive.microsoft.com/face/v1.0/persongroups/yourmeetupgroupid   -H 'cache-control: no-cache'   -H 'content-type: application/json'   -H 'ocp-apim-subscription-key: YourFaceAPISubscriptionKey'     -d '{
        "name":"yourmeetupname",
        "userData":"other-meetup-meta-data"
    }'
    ```

##### Setup the nodejs/react based visage photo app
* The attendees use this application to upload their photos 
* The services/photo folder is the root folder for this application.
* The environment setting "FACE_API_KEY" is needed by this application to persist face information for attendees in the FACE API. We got the FACE API Key in the "Create Face API person group for event" step
* You can also use the Dockerfile provided to build the docker container image, and deploy this app as a docker container. You can also use the most updated version of the maninderjit/visage container image from docker hub (The most up to date version at the time of writing these steps is maninderjit/visage:0.4).
* Once this web application is configured, up and running, The attendees need to be sent links via emails in the format below. "yourmeetupgroupid" in the link needs to be replaced by person group id created in the "Create Face API person group for event" step , and the participant_email needs to be replaced by the email id of the attendees, which off course will be different for all attendees
    ```
    https://YourAppFQDN/?event_name=yourmeetupgroupid&participant_email=attendeesemail@test.com
    ```
* The rendered page will look like

  ![Photo App with url](./images/visage-photo-app-with-url.png)

##### Attendees Upload their face photo
* On successfull upload attendees will see a tick mark as shown in the image below 

  ![Successful Upload Image](./images/visage-photo-app-upload-success.png)

* On failures, which can happen when person group with event_name does not exist in the FACE API, error is displayed in the format below

  ![Upload Image Failure](./images/visage-photo-app-upload-failure.png)

##### Installing and configuring the the UWP Checkin-app prior to checkin commencing
* This application is a modified version of the [KIOSK APP](https://github.com/Microsoft/Cognitive-Samples-IntelligentKiosk)
* The checkin-app solution can be loaded in visual studio 2015 or 2017. Once loaded the UWP app can be run
* In default configuration checkins are logged into file eventlog.txt in the pictures folder, so this file needs to be created.
* Once App is loaded we need to go Settings from the Menu and add the "FACE API Key"
* Next from the Menu we need to go into FACE Identification setup from the menu. We should see the person group created for the event. On clicking the person group we should see the name of participants who have registered for the event. On the screen where we see the participants name we need to click the play icon to train the person group to recognize the faces of the registered participants. The training can also be done via the face API REST endpoints

  ![Checkin App Training screen](./images/checkin-app-train.jpg)

##### Partipants checking in using face
* Click on the checkin app from the Menu
* Users can now start checking in using their face, by clicking on the camera icon. More than once participants can also checkin at a time

  ![Users Checking in](./images/participant-checking-in.png)

##### Adding custom logic on checkin
* You can edit the ./checkin-app/Controls/ImageWithFaceBorderUserControl.xaml.cs file. Search for TODO, you will see the following code. You can add your custom logic in the TODO section

```cs
    if (name is null)
    {
        faceUI.ShowCaptionMessage("Sorry could not identify attendee");
    }
    else
    {
        faceUI.ShowIdentificationData(age, gender, (uint)Math.Round(confidence * 100), name);

        // TODO Add code to mark attendee as checked in using <name>
        
        
    }
```
  






