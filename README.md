Remote Recycle IIS Application
======

This web app helps you remotely recycle the app pool. This has to be hosted in a server which has network access to the other servers. A member who is accessing the app should have access to the remote servers. 

http://www.nehemiahj.com/2013/10/tools-recycle-net-application-pool.html

- The server list can be added in the configuration file.
- Status Check will provide you the status of the application pool.
- This .Net application should run with an account which has admin access to all the servers.
- SMTP can be configured in the web.config.
- After each recycle, this application will send a mail to the list of configured email ids.
- This application logs the activity in the logs folder.
