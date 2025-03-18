extends CanvasLayer

var framerate = 0.0
var UPS = 0.0


func level(level_name):
	$CurrentLevel.text = 'Level: ' + level_name

func coins(num):
	$CoinCounter.text = 'Coins: ' + str(num)

func _process(delta: float) -> void:
	framerate = 1/delta
	$FpsCounter.text = 'FPS: ' + str(snapped(framerate, 0.1)) + '\nUPS: ' + str(snapped(UPS, 0.1))

func _physics_process(delta: float) -> void:
	UPS = 1/delta
