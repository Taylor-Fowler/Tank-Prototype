<?php

class DataAccess
{
	private static $instance = null;
	
	private $VENDOR = "mysql";
	private $HOST = "kunet.kingston.ac.uk";
	private $DB_NAME = "db_k1612040";
	private $DB_USER = "k1612040";
	private $DB_PASS = "password";
	private $connection = "";

	///
	/// Returns the singleton instance of the Database Access class. Creates the instance if it has not been instantiated.
	///
	public static function GetInstance()
	{
		if(self::$instance == null)
		{
			self::$instance = new DataAccess();
		}

		return self::$instance;
	}
	
	public function Login($device_id)
	{
		$query = $this->connection->prepare("SELECT * FROM `TankGame_Users` WHERE `Device_ID` = :device_id");
		$query->bindValue(":device_id", $device_id);

		if($query->execute() && $query->rowCount() > 0)
			return str_replace(array('[', ']'), '', json_encode($query->fetchAll(PDO::FETCH_ASSOC)));

		return '
			{
				"Player_ID"		: "-1",
				"Username"		: "Guest",
				"Device_ID"		: "0",
				"Games_Played"	: "0",
				"Kills"			: "0",
				"Deaths"		: "0",
				"Assists"		: "0",
				"Wins"			: "0",
				"Losses"		: "0"
			}
		';
	}
	
	public function Register($device_id, $username)
	{
		$query = $this->connection->prepare("INSERT INTO `TankGame_Users` (Username, Device_ID) VALUES (:username, :device_id)");
		$query->bindValue(":username", $username);
		$query->bindValue(":device_id", $device_id);
		$query->execute();

		return $this->Login($device_id);
	}
	
	
	private function __construct()
	{
		$this->connection = new PDO("$this->VENDOR:host=$this->HOST;dbname=$this->DB_NAME", $this->DB_USER, $this->DB_PASS);
	}
}

?>