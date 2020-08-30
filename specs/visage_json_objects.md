
#### event object schema

``` javascript
{
    //you may add something more within this object
    
    ...
    ...

    event:
    {
        id:"unique-identity-of-event",
        url:"http://link-to-meetup/event.html",
        pin:123456,
        name:"name-of-event",
        starts:"utc-date-time",
        ends:"utc-date-time",
        contact:"organizer-phone",
        email:"events@email.com",
        venue:
        {
            address_string:"complete address as a string",
            address_maps_uri:"https://goo.gl/maps/KFyThmDCaWT2"
        },
        terms:[ "first come first serve", "late comers not allowed" ],
        sessions:
        [
            {
                id:"unique-identity-of-session",                
                name:"name-of-session",
                pin:123456,
                speakers:
                [
                    {
                        id:"unique-identity-of-speaker",                        
                        name:"name-of-speaker",
                        photo:"http://storageaccount.com/speaker.png",
                        introduction:"introduction-to-speaker",
                        profiles:
                        [
                            {
                                source:"fb-or-linkedin-or-github",
                                link:"fb-or-linkedin-or-github-link"
                            }
                        ]
                    }
                ],
                room:
                {
                        name:"name-or-some-other-unique-specification",
                        floor:"floor-number-leave-null-if-not-applicable",
                        section:"building-name-or-tower-name-or-block-number",
                        location: // this needs to be populated before the event/session starts by an admin (optional if not needed)
                        {
                            latitude:0.0,
                            longitude:0.0,
                            altitude:0.0
                        }
                },
                url:"http://link-to-meetup/session.html",
                starts:"utc-date-time",
                ends:"utc-date-time",
                terms:["hands on session, bring your own device and internet"]
            }
        ]
    }
}
```

#### checkin object schema

``` javascript
{
    //you may add something more within this object
    
    ...
    ...

    attendee:
    {
        profile:
        {
            full_name:"some-name",
            checkin_pin:"phone-number-of-user-or-generated-by-admin"
        }
        event:
        {
            id:"id-or-name-of-event",
            sessions:
            [
                {
                    id:"id-or-name-of-session",
                    stay_duration_in_minutes:0,
                    checked_in:false
                }
                {
                    id:"id-or-name-of-session",
                    stay_duration_in_minutes:23,
                    checked_in:true
                },
                {
                    id:"id-or-name-of-session",
                    stay_duration_in_minutes:60,
                    checked_in:true
                }
            ]
        }
    }
}
```
