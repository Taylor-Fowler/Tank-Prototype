<?php
include_once("DataAccess.php");

if(!empty($_POST))
{
	$Device_ID = $_POST['Device_ID'];
	echo DataAccess::GetInstance()->Login($Device_ID);
}
else
{
?>

<html>
	<body>
		<form action="test.php" method="POST">
			<input name="Device_ID" type="text"></input>
		</form>
	</body>
</html>

<?php
}
?>