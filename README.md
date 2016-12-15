# blogml2wp
Console application that converts BloML xml file to Wordpress export readible by Disqus

## Usage:
BlogML2Wp inputfile domain
## Parameters:
Inputfile - name of the input file, usually BlogML.xml
Domain - http preffix, for example http://domain.net/blog. This needed to convert relative blog URLs into absolute.
## Example:
BlogML2Wp BlogML.xml http://domain.net/blog
## Scenario
 1. Clone and build this repository into BlogML2Wp.exe using Visual Studio
 2. Copy BlogML2Wp.exe into any folder, for example c:\export
 3. Export BlogEngine.NET site into BlogML.xml using admin panel (admin/#/settings/advanced)
 4. Add exported BlogML.exe into c:\export
 5. Open command line (cmd) and navigate to c:\export
 6. Run command as in example above.
 
When completed, you should find BlogML.Output.xml in the same directory that is ready to be imported into Disqus.
