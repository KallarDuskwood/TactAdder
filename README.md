# TactAdder

!!! THIS IS FOR LEGION USE ONLY !!!

This programm is used to push files from your local system to a local cdn. It will generate fileid's for new files and overwrite existing ones.
It wil **not** host these files for you. You will need to find a own Solution for this. This Code is mainly for own uses so support is an option but not a promise.
## Important
**This is an early testing build**
## Requiernments
- You need your own **local** CDN (-> https://model-changing.net/tutorials/article/118-setting-up-a-blizzlike-cdn/)
## Usage
1. Compile from source
2. Run in empty folder
3. Insert listfile ( , - seperated) - You can get it from https://wow.tools/files
4. Open Appsettings.json and make own adjustments
5. Put Files in **dataFolderPath** in Blizzlike structure (e.g. interface/gluexml/accountlogin.xml)
6. Run again
7. Profit

## Multilang support for DB2
When adding DB2 files add them to the datafolder in this pattern

**dataFolderPath**/dbfilesclient/**LOCALE**/

e.g. **dataFolderPath**/dbfilesclient/deDE/map.db2

## Config Params
-  **CdnRootDir**: Root directory of your local cdn (Root folder not "tpr\wow")
- **mysqlUser**: Mysql db username
- **mysqlPassword**: Mysql db user password
- **dataFolderPath**: Folder to read files from
- **databaseName**: Mysql database name to store FileDataId's
- **databaseAdress**: MYSQL database IP
- **exportFolderPath**: Temp folder needed for Process
## Roadmap
Features that are not yet implemented but are in the wworks.
- Mysql database to store filedataids
## Thanks
Thanks to Barncastle for creating Tact.net and Maxtorcoder for helping me out.
## Links
- https://github.com/wowdev/TACT.Net 
- https://model-changing.net/tutorials/article/136-adding-custom-files-to-a-cdn/
