# API_Service_Exchange_Rates

Database setup:
- init table in sql express server

![image](https://user-images.githubusercontent.com/109544498/224575752-ad41ee79-9e36-4f53-9a2c-90495480e606.png)

    
After setup:

![image](https://user-images.githubusercontent.com/109544498/224575580-ee224dfe-84cd-4d01-b58e-55abd8894ae0.png)

An example email after the dollar exchange rate changed:

![image](https://user-images.githubusercontent.com/109544498/224575612-84521720-ef8b-44c2-89b1-a3a9eb5758e4.png)

Email for example that the dollar rate remains the same:

![image](https://user-images.githubusercontent.com/109544498/224575646-646fccbc-bec7-429f-98b3-7e0af18908b6.png)

The service runs every 20 minutes and samples the current rates from the Bank of Israel's API and then sends an email if the dollar rate has changed and also sends an email with the rate details when the dollar rate has not changed.
The SQL server where the information is saved is a local server in my work environment which is called sql express by Microsoft.
