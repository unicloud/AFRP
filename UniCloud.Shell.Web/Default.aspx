<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml"  >
<head runat="server">
	<title>机队资源规划</title>
	<style type="text/css">
	html, body {
		height: 100%;
		overflow: auto;
	}
	body {
		padding: 0;
		margin: 0;
	}
	#silverlightControlHost {
		height: 100%;
		text-align:center;
	}
	</style>
	<script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript" src="Scripts/jquery-1.7.1.js"></script>
	<script type="text/javascript">
	    function onSilverlightError(sender, args) {
	        var appSource = "";
	        if (sender != null && sender != 0) {
	            appSource = sender.getHost().Source;
	        }
			
	        var errorType = args.ErrorType;
	        var iErrorCode = args.ErrorCode;

	        if (errorType == "ImageError" || errorType == "MediaError") {
	            return;
	        }

	        var errMsg = "Unhandled Error in Silverlight Application " +  appSource + "\n" ;

	        errMsg += "Code: "+ iErrorCode + "    \n";
	        errMsg += "Category: " + errorType + "       \n";
	        errMsg += "Message: " + args.ErrorMessage + "     \n";

	        if (errorType == "ParserError") {
	            errMsg += "File: " + args.xamlFile + "     \n";
	            errMsg += "Line: " + args.lineNumber + "     \n";
	            errMsg += "Position: " + args.charPosition + "     \n";
	        }
	        else if (errorType == "RuntimeError") {           
	            if (args.lineNumber != 0) {
	                errMsg += "Line: " + args.lineNumber + "     \n";
	                errMsg += "Position: " +  args.charPosition + "     \n";
	            }
	            errMsg += "MethodName: " + args.methodName + "     \n";
	        }

	        throw new Error(errMsg);
	    }
	</script>
    <script type="text/javascript">
        $(document).ready(function () {
            var thisU1 = window.location.protocol; // http:
            var thisU2 = window.location.host;   // localhost:81
            var thisU3 = "/Silverlight.zip";
            var fileUrl = (thisU1 + "//" + thisU2 + thisU3);
            $("#silverlightPlus").attr("href", fileUrl);
        })
    </script>
</head>
<body>
	<form id="form1" runat="server" style="height:100%">
	<div id="silverlightControlHost">
		<object data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
		  <param name="source" value="ClientBin/UniCloud.Shell.xap"/>
		  <param name="onError" value="onSilverlightError" />
		  <param name="background" value="white" />
		  <param name="minRuntimeVersion" value="5.0.61118.0" />
		  <param name="autoUpgrade" value="true" />
		  <a id="silverlightPlus"  style="text-decoration:none">
			  <img src="Images/SilverlightPlusImg.png" alt="获取Silverlight插件" style="border-style:none"/>
		  </a>
		</object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe></div>
	</form>
</body>
</html>
