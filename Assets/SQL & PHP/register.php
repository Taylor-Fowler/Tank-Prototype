<?php
include_once("DataAccess.php");

if(!empty($_POST))
{
	$Device_ID = $_POST['Device_ID'];
	$Username = $_POST['Username'];
	echo DataAccess::GetInstance()->Register($Device_ID, $Username);
}
else
{
?>

<html>
	<body>
		<form action="register.php" method="POST">
			<input name="Device_ID" type="text"></input>
			<input name="Username" type="text"></input>
		</form>
	</body>
</html>

<?php
}
?>