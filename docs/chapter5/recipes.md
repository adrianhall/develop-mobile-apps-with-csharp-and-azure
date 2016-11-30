This section is dedicated to exploring various common recipes for push notifications and how we can achieve those recipes through cross-platform code.

## Marketing Push

The most common requirement for push notifications is to alert users of a special offer or other marketing information.  The general idea is that
the marketing person will create a "campaign" that includes a push notification.  When the user receives the push notification, they will accept
it.  If a user accepts the push notification, the mobile app will deep-link into a specific page and store the fact that the user viewed the page
within the database.

## Geofenced Push

This is a variation on the marketing push.  You want to send a marketing message, but only to those users that are within a certain geography.
This is known as geofencing.

## Push to Sync

Sometimes, you want to alert the user that there is something new for that user.  When the user is alerted, acceptance of the push notification
indicates that the user wants to go to the app and synchronize the database before viewing the data.

## Secure Push

Push notifications are insecure.  They appear on the front page of the lock screen and anyone can open them in a multi-user environment (where a
mobile device is shared by a community of users).  In these cases, you may want to ensure that the push notification is only opened by the user
for which it was intended.


