## Survival 1.5

- 프로젝트 주제 : 시뮬레이션 게임을 통한 기후위기 인식 증대

- 개발기간 : 2020.08.31 ~ 2020.12.10

- 개발플랫폼 : UNITY 3d, visual studio 2019

- 개발언어 : c#, c++, R	

- 시연영상 : https://www.youtube.com/watch?v=unwO_Dq87H4

## 프로젝트 구조

![sv1 5_structure](https://user-images.githubusercontent.com/14107670/139583419-c3952111-e568-462a-8957-4be45944d227.jpg)

- 클라이언트(유니티)와 동적   라이브러리(.so)간 Interface Method를 통해 데이터 연동

- 클라이언트에서 Start 함수 실행 시 InitGame() 및 PlayGame()을 통해 데이터 초기화 및 sub thread 생성

- 유니티의 지속적으로 실행되는 함수인 Update()를 통해 반복주기마다 데이터에 대해 get, set 실행

- 실시간으로 반영하기 위해 main thread(유니티)의 주기는 지연시간을 고려하여 sub thread의 절반으로 함

- 게임에 대한 종료가 호출될 시 GameEnd()를 통해 sub thread를 종료


