using System;
using System.Collections.Generic;
using UnityEngine;

public class RenjuRuleChecker
{
    private Constants.MarkerType[,] _board;
    private TurnManager _turnManager;
    private bool[,] _forbiddenPositions;
    
    // 패턴 분석용 상수
    private const char BLACK = 'B';
    private const char WHITE = 'W';
    private const char EMPTY = '0';
    private const char OUT_OF_BOUNDS = 'X';
    
    // 패턴 재사용을 위한 StringBuilder (메모리 할당 최적화)
    private System.Text.StringBuilder _patternBuilder;

    public RenjuRuleChecker(Constants.MarkerType[,] board, TurnManager turnManager)
    {
        _board = board;
        _turnManager = turnManager;
        _forbiddenPositions = new bool[_board.GetLength(0), _board.GetLength(1)];
        _patternBuilder = new System.Text.StringBuilder(30); // 최대 패턴 길이 예상하여 초기 용량 할당
    }

    // 백돌 차례가 끝난 후 호출하여 모든 금수 위치 계산
    public void CalculateForbiddenPositions()
    {
        // 1단계: 기본 금수 계산
        bool[,] tempForbiddenPositions = new bool[_board.GetLength(0), _board.GetLength(1)];
        CalculateBasicForbiddenPositions(tempForbiddenPositions);
        
        // 2단계: 33 금수 재검사 (빈 공간의 금수 여부 고려)
        bool[,] finalForbiddenPositions = new bool[_board.GetLength(0), _board.GetLength(1)];
        RecalculateWithForbiddenCheck(tempForbiddenPositions, finalForbiddenPositions);
        
        // 3단계: 금수 해소 조건 적용
        ApplyForbiddenExemptions(finalForbiddenPositions);
        
        // 최종 금수 위치 배열에 복사
        CopyForbiddenPositions(finalForbiddenPositions);
    }
    
    // 1단계: 기본 금수 계산
    private void CalculateBasicForbiddenPositions(bool[,] tempForbiddenPositions)
    {
        for (int x = 0; x < _board.GetLength(0); x++)
        {
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                if (_board[x, y] != Constants.MarkerType.None) continue;
                Vector2Int pos = new Vector2Int(x, y);
                tempForbiddenPositions[x, y] = CheckBasicForbiddenRules(pos);
            }
        }
    }
    
    // 2단계: 33 금수 재검사 (빈 공간의 금수 여부 고려)
    private void RecalculateWithForbiddenCheck(bool[,] tempForbiddenPositions, bool[,] finalForbiddenPositions)
    {
        for (int x = 0; x < _board.GetLength(0); x++)
        {
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                if (_board[x, y] != Constants.MarkerType.None) 
                {
                    finalForbiddenPositions[x, y] = false;
                    continue;
                }
                
                Vector2Int pos = new Vector2Int(x, y);
                
                if (CheckOverline(pos))
                {
                    finalForbiddenPositions[x, y] = true;
                    continue;
                }
                
                bool hasOpenFour = HasOpenFour(pos);
                
                if (!hasOpenFour && CheckOpenThreeWithForbiddenCheck(pos, tempForbiddenPositions))
                {
                    finalForbiddenPositions[x, y] = true;
                    continue;
                }
                
                if (CheckOpenFour(pos))
                {
                    finalForbiddenPositions[x, y] = true;
                    continue;
                }
                
                finalForbiddenPositions[x, y] = false;
            }
        }
    }
    
    // 3단계: 금수 해소 조건 적용
    private void ApplyForbiddenExemptions(bool[,] finalForbiddenPositions)
    {
        for (int x = 0; x < _board.GetLength(0); x++)
        {
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                if (_board[x, y] != Constants.MarkerType.None || !finalForbiddenPositions[x, y]) continue;
                
                Vector2Int pos = new Vector2Int(x, y);
                
                // 금수 해소 조건 확인
                if (CheckFive(pos) || CheckBlockingFour(pos) || CheckFutureSixInRow(pos))
                {
                    finalForbiddenPositions[x, y] = false;
                }
            }
        }
    }
    
    // 최종 금수 위치 배열에 복사
    private void CopyForbiddenPositions(bool[,] finalForbiddenPositions)
    {
        for (int x = 0; x < _board.GetLength(0); x++)
        {
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                _forbiddenPositions[x, y] = finalForbiddenPositions[x, y];
            }
        }
    }

    // 기본 금수 규칙 검사 (33, 44, 장목)
    private bool CheckBasicForbiddenRules(Vector2Int position)
    {
        if (CheckFive(position)) return false;
        if (CheckOverline(position)) return true;
        
        bool hasOpenFour = HasOpenFour(position);
        
        if (CheckOpenFour(position)) return true;
        if (!hasOpenFour && CheckOpenThree(position)) return true;
        
        return false;
    }
    
    // 패턴을 문자열로 추출하는 함수 (StringBuilder 재사용하여 최적화)
    private string ExtractPattern(Vector2Int position, Vector2Int direction, int length)
    {
        if (length % 2 == 0) length += 1; // 홀수로 맞춤
        
        int halfLength = length / 2;
        _patternBuilder.Clear();
        
        // 중앙 위치에 흑돌 (현재 검사 위치)
        _patternBuilder.Append(BLACK);
        
        // 정방향 추출
        for (int i = 1; i <= halfLength; i++)
        {
            Vector2Int pos = position + direction * i;
            if (IsInBoardRange(pos))
            {
                if (_board[pos.x, pos.y] == Constants.MarkerType.Black)
                    _patternBuilder.Append(BLACK);
                else if (_board[pos.x, pos.y] == Constants.MarkerType.White)
                    _patternBuilder.Append(WHITE);
                else
                    _patternBuilder.Append(EMPTY);
            }
            else
                _patternBuilder.Append(OUT_OF_BOUNDS);
        }
        
        // 역방향 추출
        for (int i = 1; i <= halfLength; i++)
        {
            Vector2Int pos = position - direction * i;
            if (IsInBoardRange(pos))
            {
                char stone;
                if (_board[pos.x, pos.y] == Constants.MarkerType.Black)
                    stone = BLACK;
                else if (_board[pos.x, pos.y] == Constants.MarkerType.White)
                    stone = WHITE;
                else
                    stone = EMPTY;
                
                _patternBuilder.Insert(0, stone);
            }
            else
                _patternBuilder.Insert(0, OUT_OF_BOUNDS);
        }
        
        return _patternBuilder.ToString();
    }
    
    // 열린 3 패턴 검사 (빈 공간의 금수 여부 고려 가능)
    private bool IsOpenThreePattern(string pattern, Vector2Int position = default, Vector2Int direction = default, bool[,] forbiddenMap = null)
{
    bool checkForbidden = (position != default && direction != default && forbiddenMap != null);
    int patternLength = pattern.Length;
    int middle = patternLength / 2; // 중앙 인덱스

    // 패턴에서 흑돌 3개로 이루어진 묶음 찾기
    for (int i = 0; i < patternLength - 2; i++)
    {
        // 연속된 3개 흑돌 패턴 (BBB)
        if (i + 2 < patternLength && pattern[i] == 'B' && pattern[i + 1] == 'B' && pattern[i + 2] == 'B')
        {
            if (i <= middle && middle <= i + 2)
            {
                // 패턴 양쪽이 공백인지 확인
                if (i > 0 && i + 3 < patternLength && pattern[i - 1] == '0' && pattern[i + 3] == '0')
                {
                    // 패턴 주변에 추가 흑돌이 없는지 확인 (진짜 열린 3인지)
                    bool extraBlackStoneLeft = (i > 1 && pattern[i - 2] == 'B');
                    bool extraBlackStoneRight = (i + 4 < patternLength && pattern[i + 4] == 'B');

                    // 추가 흑돌이 있으면 열린 3이 아님 (다른 패턴의 일부일 수 있음)
                    if (extraBlackStoneLeft || extraBlackStoneRight)
                        continue;

                    // 다음 칸이 백돌인지 확인
                    bool leftBlocked = (i > 1 && pattern[i - 2] == 'W');
                    bool rightBlocked = (i + 4 < patternLength && pattern[i + 4] == 'W');

                    // 양쪽 모두 백돌로 막혀있으면 닫힌 패턴, 그렇지 않으면 열린 패턴
                    if (!(leftBlocked && rightBlocked))
                    {
                        // 한 수를 더 두면 열린4가 되는지 확인
                        bool canFormOpenFour = false;

                        // 왼쪽 공백에 돌을 놓는 경우
                        if (!leftBlocked)
                        {
                            bool canCheckLeft = true;
                            
                            // 금수 확인이 필요한 경우
                            if (checkForbidden)
                            {
                                // 왼쪽 공백 위치 계산
                                Vector2Int leftEmptyPos = position - direction * (middle - i + 1);
                                
                                if (!IsInBoardRange(leftEmptyPos) || forbiddenMap[leftEmptyPos.x, leftEmptyPos.y])
                                {
                                    canCheckLeft = false;
                                }
                            }
                            
                            if (canCheckLeft)
                            {
                                // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                char[] simulationChars = pattern.ToCharArray();
                                simulationChars[i - 1] = 'B';
                                string leftSimulation = new string(simulationChars);
                                if (IsOpenFourPattern(leftSimulation, position, direction, forbiddenMap))
                                    canFormOpenFour = true;
                            }
                        }

                        // 오른쪽 공백에 돌을 놓는 경우
                        if (!canFormOpenFour && !rightBlocked)
                        {
                            bool canCheckRight = true;
                            
                            // 금수 확인이 필요한 경우
                            if (checkForbidden)
                            {
                                // 오른쪽 공백 위치 계산
                                Vector2Int rightEmptyPos = position + direction * (i + 3 - middle);
                                
                                if (!IsInBoardRange(rightEmptyPos) || forbiddenMap[rightEmptyPos.x, rightEmptyPos.y])
                                {
                                    canCheckRight = false;
                                }
                            }
                            
                            if (canCheckRight)
                            {
                                // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                char[] simulationChars = pattern.ToCharArray();
                                simulationChars[i + 3] = 'B';
                                string rightSimulation = new string(simulationChars);
                                if (IsOpenFourPattern(rightSimulation, position, direction, forbiddenMap))
                                    canFormOpenFour = true;
                            }
                        }

                        // 열린4를 형성할 수 있는 경우에만 열린3으로 인정
                        if (canFormOpenFour)
                        {
                            Debug.Log(
                                $"Found open three pattern: BBB at index {i}, leftBlocked: {leftBlocked}, rightBlocked: {rightBlocked}");
                            return true;
                        }
                    }
                }
            }
        }

        // 한 칸 공백이 있는 3개 흑돌 패턴 (B0BB 또는 BB0B)
        if (i + 3 < patternLength)
        {
            // B0BB 패턴
            if (pattern[i] == 'B' && pattern[i + 1] == '0' && pattern[i + 2] == 'B' && pattern[i + 3] == 'B')
            {
                if (i <= middle && middle <= i + 3)
                {
                    // 패턴 양쪽이 공백인지 확인
                    if (i > 0 && i + 4 < patternLength && pattern[i - 1] == '0' && pattern[i + 4] == '0')
                    {
                        // 패턴 주변에 추가 흑돌이 없는지 확인
                        bool extraBlackStoneLeft = (i > 1 && pattern[i - 2] == 'B');
                        bool extraBlackStoneRight = (i + 5 < patternLength && pattern[i + 5] == 'B');

                        // 추가 흑돌이 있으면 열린 3이 아님
                        if (extraBlackStoneLeft || extraBlackStoneRight)
                            continue;

                        // 다음 칸이 백돌인지 확인
                        bool leftBlocked = (i > 1 && pattern[i - 2] == 'W');
                        bool rightBlocked = (i + 5 < patternLength && pattern[i + 5] == 'W');

                        // 양쪽 모두 백돌로 막혀있으면 닫힌 패턴, 그렇지 않으면 열린 패턴
                        if (!(leftBlocked && rightBlocked))
                        {
                            // 한 수를 더 두면 열린4가 되는지 확인
                            bool canFormOpenFour = false;

                            // 왼쪽 공백에 돌을 놓는 경우
                            if (!leftBlocked)
                            {
                                bool canCheckLeft = true;
                                
                                // 금수 확인이 필요한 경우
                                if (checkForbidden)
                                {
                                    // 왼쪽 공백 위치 계산
                                    Vector2Int leftEmptyPos = position - direction * (middle - i + 1);
                                    
                                    if (!IsInBoardRange(leftEmptyPos) || forbiddenMap[leftEmptyPos.x, leftEmptyPos.y])
                                    {
                                        canCheckLeft = false;
                                    }
                                }
                                
                                if (canCheckLeft)
                                {
                                    // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                    char[] simulationChars = pattern.ToCharArray();
                                    simulationChars[i - 1] = 'B';
                                    string leftSimulation = new string(simulationChars);
                                    if (IsOpenFourPattern(leftSimulation, position, direction, forbiddenMap))
                                        canFormOpenFour = true;
                                }
                            }

                            // 오른쪽 공백에 돌을 놓는 경우
                            if (!canFormOpenFour && !rightBlocked)
                            {
                                bool canCheckRight = true;
                                
                                // 금수 확인이 필요한 경우
                                if (checkForbidden)
                                {
                                    // 오른쪽 공백 위치 계산
                                    Vector2Int rightEmptyPos = position + direction * (i + 4 - middle);
                                    
                                    if (!IsInBoardRange(rightEmptyPos) || forbiddenMap[rightEmptyPos.x, rightEmptyPos.y])
                                    {
                                        canCheckRight = false;
                                    }
                                }
                                
                                if (canCheckRight)
                                {
                                    // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                    char[] simulationChars = pattern.ToCharArray();
                                    simulationChars[i + 4] = 'B';
                                    string rightSimulation = new string(simulationChars);
                                    if (IsOpenFourPattern(rightSimulation, position, direction, forbiddenMap))
                                        canFormOpenFour = true;
                                }
                            }

                            // 중간 공백에 돌을 놓는 경우
                            if (!canFormOpenFour)
                            {
                                bool canCheckMiddle = true;
                                
                                // 금수 확인이 필요한 경우
                                if (checkForbidden)
                                {
                                    // 중간 공백 위치 계산
                                    Vector2Int middleEmptyPos = position + direction * (i + 1 - middle);
                                    
                                    if (!IsInBoardRange(middleEmptyPos) || forbiddenMap[middleEmptyPos.x, middleEmptyPos.y])
                                    {
                                        canCheckMiddle = false;
                                    }
                                }
                                
                                if (canCheckMiddle)
                                {
                                    // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                    char[] simulationChars = pattern.ToCharArray();
                                    simulationChars[i + 1] = 'B';
                                    string middleSimulation = new string(simulationChars);
                                    if (IsOpenFourPattern(middleSimulation, position, direction, forbiddenMap))
                                        canFormOpenFour = true;
                                }
                            }

                            // 열린4를 형성할 수 있는 경우에만 열린3으로 인정
                            if (canFormOpenFour)
                            {
                                Debug.Log(
                                    $"Found open three pattern: B0BB at index {i}, leftBlocked: {leftBlocked}, rightBlocked: {rightBlocked}");
                                return true;
                            }
                        }
                    }
                }
            }

            // BB0B 패턴
            if (pattern[i] == 'B' && pattern[i + 1] == 'B' && pattern[i + 2] == '0' && pattern[i + 3] == 'B')
            {
                if (i <= middle && middle <= i + 3)
                {
                    // 패턴 양쪽이 공백인지 확인
                    if (i > 0 && i + 4 < patternLength && pattern[i - 1] == '0' && pattern[i + 4] == '0')
                    {
                        // 패턴 주변에 추가 흑돌이 없는지 확인
                        bool extraBlackStoneLeft = (i > 1 && pattern[i - 2] == 'B');
                        bool extraBlackStoneRight = (i + 5 < patternLength && pattern[i + 5] == 'B');

                        // 추가 흑돌이 있으면 열린 3이 아님
                        if (extraBlackStoneLeft || extraBlackStoneRight)
                            continue;

                        // 다음 칸이 백돌인지 확인
                        bool leftBlocked = (i > 1 && pattern[i - 2] == 'W');
                        bool rightBlocked = (i + 5 < patternLength && pattern[i + 5] == 'W');

                        // 양쪽 모두 백돌로 막혀있으면 닫힌 패턴, 그렇지 않으면 열린 패턴
                        if (!(leftBlocked && rightBlocked))
                        {
                            // 한 수를 더 두면 열린4가 되는지 확인
                            bool canFormOpenFour = false;

                            // 왼쪽 공백에 돌을 놓는 경우
                            if (!leftBlocked)
                            {
                                bool canCheckLeft = true;
                                
                                // 금수 확인이 필요한 경우
                                if (checkForbidden)
                                {
                                    // 왼쪽 공백 위치 계산
                                    Vector2Int leftEmptyPos = position - direction * (middle - i + 1);
                                    
                                    if (!IsInBoardRange(leftEmptyPos) || forbiddenMap[leftEmptyPos.x, leftEmptyPos.y])
                                    {
                                        canCheckLeft = false;
                                    }
                                }
                                
                                if (canCheckLeft)
                                {
                                    // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                    char[] simulationChars = pattern.ToCharArray();
                                    simulationChars[i - 1] = 'B';
                                    string leftSimulation = new string(simulationChars);
                                    if (IsOpenFourPattern(leftSimulation, position, direction, forbiddenMap))
                                        canFormOpenFour = true;
                                }
                            }

                            // 오른쪽 공백에 돌을 놓는 경우
                            if (!canFormOpenFour && !rightBlocked)
                            {
                                bool canCheckRight = true;
                                
                                // 금수 확인이 필요한 경우
                                if (checkForbidden)
                                {
                                    // 오른쪽 공백 위치 계산
                                    Vector2Int rightEmptyPos = position + direction * (i + 4 - middle);
                                    
                                    if (!IsInBoardRange(rightEmptyPos) || forbiddenMap[rightEmptyPos.x, rightEmptyPos.y])
                                    {
                                        canCheckRight = false;
                                    }
                                }
                                
                                if (canCheckRight)
                                {
                                    // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                    char[] simulationChars = pattern.ToCharArray();
                                    simulationChars[i + 4] = 'B';
                                    string rightSimulation = new string(simulationChars);
                                    if (IsOpenFourPattern(rightSimulation, position, direction, forbiddenMap))
                                        canFormOpenFour = true;
                                }
                            }

                            // 중간 공백에 돌을 놓는 경우
                            if (!canFormOpenFour)
                            {
                                bool canCheckMiddle = true;
                                
                                // 금수 확인이 필요한 경우
                                if (checkForbidden)
                                {
                                    // 중간 공백 위치 계산
                                    Vector2Int middleEmptyPos = position + direction * (i + 2 - middle);
                                    
                                    if (!IsInBoardRange(middleEmptyPos) || forbiddenMap[middleEmptyPos.x, middleEmptyPos.y])
                                    {
                                        canCheckMiddle = false;
                                    }
                                }
                                
                                if (canCheckMiddle)
                                {
                                    // 공백에 돌을 놓고 열린4가 되는지 시뮬레이션
                                    char[] simulationChars = pattern.ToCharArray();
                                    simulationChars[i + 2] = 'B';
                                    string middleSimulation = new string(simulationChars);
                                    if (IsOpenFourPattern(middleSimulation, position, direction, forbiddenMap))
                                        canFormOpenFour = true;
                                }
                            }

                            // 열린4를 형성할 수 있는 경우에만 열린3으로 인정
                            if (canFormOpenFour)
                            {
                                Debug.Log(
                                    $"Found open three pattern: BB0B at index {i}, leftBlocked: {leftBlocked}, rightBlocked: {rightBlocked}");
                                return true;
                            }
                        }
                    }
                }
            }
        }
    }

    return false;
}
    
    // 열린 4 패턴 검사
    private bool IsOpenFourPattern(string pattern, Vector2Int position = default, Vector2Int direction = default, bool[,] forbiddenMap = null)
{
    bool checkForbidden = (position != default && direction != default && forbiddenMap != null);
    int patternLength = pattern.Length;
    int middle = patternLength / 2; // 중앙 인덱스

    // 패턴에서 흑돌 4개로 이루어진 묶음 찾기
    for (int i = 0; i < patternLength - 3; i++)
    {
        // 연속된 4개 흑돌 패턴 (BBBB)
        if (i + 3 < patternLength &&
            pattern[i] == 'B' && pattern[i + 1] == 'B' && pattern[i + 2] == 'B' && pattern[i + 3] == 'B')
        {
            if (i <= middle && middle <= i + 3)
            {
                // 최소 한쪽 끝이 공백인지 확인
                bool leftOpen = (i > 0 && pattern[i - 1] == '0');
                bool rightOpen = (i + 4 < patternLength && pattern[i + 4] == '0');

                if (leftOpen || rightOpen)
                {
                    // 다음 칸이 백돌인지 확인
                    bool leftBlocked = (i > 1 && pattern[i - 2] == 'W');
                    bool rightBlocked = (i + 5 < patternLength && pattern[i + 5] == 'W');

                    // 공백에 놓을 경우 금수인지 확인
                    bool canPlaceLeft = true;
                    bool canPlaceRight = true;

                    if (checkForbidden)
                    {
                        if (leftOpen)
                        {
                            Vector2Int leftEmptyPos = position - direction * (middle - i + 1);
                            if (!IsInBoardRange(leftEmptyPos) || forbiddenMap[leftEmptyPos.x, leftEmptyPos.y])
                            {
                                canPlaceLeft = false;
                            }
                        }

                        if (rightOpen)
                        {
                            Vector2Int rightEmptyPos = position + direction * (i + 4 - middle);
                            if (!IsInBoardRange(rightEmptyPos) || forbiddenMap[rightEmptyPos.x, rightEmptyPos.y])
                            {
                                canPlaceRight = false;
                            }
                        }
                    }

                    // 양쪽 모두 백돌로 막혀있을 때만 닫힌 패턴으로 간주하되, 금수 검사 적용
                    if (!(leftBlocked && rightBlocked) && (canPlaceLeft || canPlaceRight))
                        return true;
                }
            }
        }

        // 한 칸 공백이 있는 4개 흑돌 패턴 (B0BBB, BB0BB, BBB0B)
        if (i + 4 < patternLength)
        {
            // B0BBB 패턴
            if (pattern[i] == 'B' && pattern[i + 1] == '0' &&
                pattern[i + 2] == 'B' && pattern[i + 3] == 'B' && pattern[i + 4] == 'B')
            {
                if (i <= middle && middle <= i + 4)
                {
                    // 최소 한쪽 끝이 공백인지 확인
                    bool leftOpen = (i > 0 && pattern[i - 1] == '0');
                    bool rightOpen = (i + 5 < patternLength && pattern[i + 5] == '0');

                    if (leftOpen || rightOpen)
                    {
                        // 다음 칸이 백돌인지 확인
                        bool leftBlocked = (i > 1 && pattern[i - 2] == 'W');
                        bool rightBlocked = (i + 6 < patternLength && pattern[i + 6] == 'W');

                        // 공백에 놓을 경우 금수인지 확인
                        bool canPlaceLeft = true;
                        bool canPlaceRight = true;
                        bool canPlaceMiddle = true;

                        if (checkForbidden)
                        {
                            if (leftOpen)
                            {
                                Vector2Int leftEmptyPos = position - direction * (middle - i + 1);
                                if (!IsInBoardRange(leftEmptyPos) || forbiddenMap[leftEmptyPos.x, leftEmptyPos.y])
                                {
                                    canPlaceLeft = false;
                                }
                            }

                            if (rightOpen)
                            {
                                Vector2Int rightEmptyPos = position + direction * (i + 5 - middle);
                                if (!IsInBoardRange(rightEmptyPos) || forbiddenMap[rightEmptyPos.x, rightEmptyPos.y])
                                {
                                    canPlaceRight = false;
                                }
                            }

                            // 중간 공백 위치 확인
                            Vector2Int middleEmptyPos = position + direction * (i + 1 - middle);
                            if (!IsInBoardRange(middleEmptyPos) || forbiddenMap[middleEmptyPos.x, middleEmptyPos.y])
                            {
                                canPlaceMiddle = false;
                            }
                        }

                        // 양쪽 모두 백돌로 막혀있을 때만 닫힌 패턴으로 간주하되, 금수 검사 적용
                        if (!(leftBlocked && rightBlocked) && (canPlaceLeft || canPlaceRight || canPlaceMiddle))
                            return true;
                    }
                }
            }

            // BB0BB 패턴
            if (pattern[i] == 'B' && pattern[i + 1] == 'B' && pattern[i + 2] == '0' &&
                pattern[i + 3] == 'B' && pattern[i + 4] == 'B')
            {
                if (i <= middle && middle <= i + 4)
                {
                    // 최소 한쪽 끝이 공백인지 확인
                    bool leftOpen = (i > 0 && pattern[i - 1] == '0');
                    bool rightOpen = (i + 5 < patternLength && pattern[i + 5] == '0');

                    if (leftOpen || rightOpen)
                    {
                        // 다음 칸이 백돌인지 확인
                        bool leftBlocked = (i > 1 && pattern[i - 2] == 'W');
                        bool rightBlocked = (i + 6 < patternLength && pattern[i + 6] == 'W');

                        // 공백에 놓을 경우 금수인지 확인
                        bool canPlaceLeft = true;
                        bool canPlaceRight = true;
                        bool canPlaceMiddle = true;

                        if (checkForbidden)
                        {
                            if (leftOpen)
                            {
                                Vector2Int leftEmptyPos = position - direction * (middle - i + 1);
                                if (!IsInBoardRange(leftEmptyPos) || forbiddenMap[leftEmptyPos.x, leftEmptyPos.y])
                                {
                                    canPlaceLeft = false;
                                }
                            }

                            if (rightOpen)
                            {
                                Vector2Int rightEmptyPos = position + direction * (i + 5 - middle);
                                if (!IsInBoardRange(rightEmptyPos) || forbiddenMap[rightEmptyPos.x, rightEmptyPos.y])
                                {
                                    canPlaceRight = false;
                                }
                            }

                            // 중간 공백 위치 확인
                            Vector2Int middleEmptyPos = position + direction * (i + 2 - middle);
                            if (!IsInBoardRange(middleEmptyPos) || forbiddenMap[middleEmptyPos.x, middleEmptyPos.y])
                            {
                                canPlaceMiddle = false;
                            }
                        }

                        // 양쪽 모두 백돌로 막혀있을 때만 닫힌 패턴으로 간주하되, 금수 검사 적용
                        if (!(leftBlocked && rightBlocked) && (canPlaceLeft || canPlaceRight || canPlaceMiddle))
                            return true;
                    }
                }
            }

            // BBB0B 패턴
            if (pattern[i] == 'B' && pattern[i + 1] == 'B' && pattern[i + 2] == 'B' &&
                pattern[i + 3] == '0' && pattern[i + 4] == 'B')
            {
                if (i <= middle && middle <= i + 4)
                {
                    // 최소 한쪽 끝이 공백인지 확인
                    bool leftOpen = (i > 0 && pattern[i - 1] == '0');
                    bool rightOpen = (i + 5 < patternLength && pattern[i + 5] == '0');

                    if (leftOpen || rightOpen)
                    {
                        // 다음 칸이 백돌인지 확인
                        bool leftBlocked = (i > 1 && pattern[i - 2] == 'W');
                        bool rightBlocked = (i + 6 < patternLength && pattern[i + 6] == 'W');

                        // 공백에 놓을 경우 금수인지 확인
                        bool canPlaceLeft = true;
                        bool canPlaceRight = true;
                        bool canPlaceMiddle = true;

                        if (checkForbidden)
                        {
                            if (leftOpen)
                            {
                                Vector2Int leftEmptyPos = position - direction * (middle - i + 1);
                                if (!IsInBoardRange(leftEmptyPos) || forbiddenMap[leftEmptyPos.x, leftEmptyPos.y])
                                {
                                    canPlaceLeft = false;
                                }
                            }

                            if (rightOpen)
                            {
                                Vector2Int rightEmptyPos = position + direction * (i + 5 - middle);
                                if (!IsInBoardRange(rightEmptyPos) || forbiddenMap[rightEmptyPos.x, rightEmptyPos.y])
                                {
                                    canPlaceRight = false;
                                }
                            }

                            // 중간 공백 위치 확인
                            Vector2Int middleEmptyPos = position + direction * (i + 3 - middle);
                            if (!IsInBoardRange(middleEmptyPos) || forbiddenMap[middleEmptyPos.x, middleEmptyPos.y])
                            {
                                canPlaceMiddle = false;
                            }
                        }

                        // 양쪽 모두 백돌로 막혀있을 때만 닫힌 패턴으로 간주하되, 금수 검사 적용
                        if (!(leftBlocked && rightBlocked) && (canPlaceLeft || canPlaceRight || canPlaceMiddle))
                            return true;
                    }
                }
            }
        }
    }

    return false;
}

    // 열린 3 검사 함수
    private bool CheckOpenThree(Vector2Int position)
    {
        SimulatePlacement(position, Constants.MarkerType.Black, out Constants.MarkerType originalValue);
    
        int openThreeCount = 0;
        Vector2Int[] directions = GetDirections();
    
        foreach (Vector2Int dir in directions)
        {
            string pattern = ExtractPattern(position, dir, 9);
            if (IsOpenThreePattern(pattern)) // 금수 검사 없이 기본 호출
            {
                openThreeCount++;
                if (openThreeCount >= 2)
                    break;
            }
        }
    
        // 원래 상태로 복원
        _board[position.x, position.y] = originalValue;
    
        return openThreeCount >= 2; // 열린 3이 2개 이상이면 삼삼 금수
    }
    
    // 33 금수 검사 (빈 공간의 금수 여부 고려)
    private bool CheckOpenThreeWithForbiddenCheck(Vector2Int position, bool[,] forbiddenMap)
    {
        SimulatePlacement(position, Constants.MarkerType.Black, out Constants.MarkerType originalValue);
    
        Vector2Int[] directions = GetDirections();
        int openThreeCount = 0;
    
        foreach (Vector2Int dir in directions)
        {
            // 패턴 추출 전에 이 방향으로 금수 위치가 있는지 먼저 확인
            bool hasForbiddenInDirection = false;
        
            for (int dist = -4; dist <= 4; dist++)
            {
                if (dist == 0) continue; // 중앙 위치는 건너뜀 (검사 위치)
            
                Vector2Int checkPos = position + dir * dist;
                if (IsInBoardRange(checkPos) && _board[checkPos.x, checkPos.y] == Constants.MarkerType.None && 
                    forbiddenMap[checkPos.x, checkPos.y])
                {
                    hasForbiddenInDirection = true;
                    break;
                }
            }
        
            if (hasForbiddenInDirection)
            {
                continue; // 이 방향에 금수 위치가 있으면 건너뜀
            }
        
            string pattern = ExtractPattern(position, dir, 9);
        
            if (IsOpenThreePattern(pattern))
            {
                openThreeCount++;
                if (openThreeCount >= 2)
                {
                    _board[position.x, position.y] = originalValue;
                    Debug.Log($"Position {position}: Creates two or more open threes with forbidden check");
                    return true;
                }
            }
        }
    
        _board[position.x, position.y] = originalValue;
        return false;
    }
    
    // 해당 위치에 열린 4가 하나라도 있는지 확인
    private bool HasOpenFour(Vector2Int position)
    {
        SimulatePlacement(position, Constants.MarkerType.Black, out Constants.MarkerType originalValue);
    
        bool hasOpenFour = CheckAnyDirectionPattern(position, pattern => IsOpenFourPattern(pattern));
    
        _board[position.x, position.y] = originalValue;
    
        return hasOpenFour;
    }
    
    // 열린 4 갯수 확인 함수 (사사 금수용)
    private bool CheckOpenFour(Vector2Int position)
    {
        SimulatePlacement(position, Constants.MarkerType.Black, out Constants.MarkerType originalValue);
    
        Vector2Int[] directions = GetDirections();
    
        // 한 라인에서 44 패턴 검사
        foreach (Vector2Int dir in directions)
        {
            string pattern = ExtractPattern(position, dir, 11);
        
            if (CheckDoubleFourInLine(pattern))
            {
                _board[position.x, position.y] = originalValue;
                return true;
            }
        }
    
        // 서로 다른 라인에서 44 검사
        int openFourCount = CountPatternDirections(position, pattern => IsOpenFourPattern(pattern));
    
        _board[position.x, position.y] = originalValue;
    
        return openFourCount >= 2; // 열린 4가 2개 이상이면 사사 금수
    }
    
    // 한 라인에서 44(사사) 패턴 확인 - 최적화된 버전
    private bool CheckDoubleFourInLine(string pattern)
    {
        // 44 패턴 검사 로직
        int fourPatternCount = 0;
        HashSet<int> detectedFourPositions = new HashSet<int>(); // 중복 방지

        int patternLength = pattern.Length;

        // 1. 연속된 4개 흑돌 패턴 (BBBB)
        for (int i = 0; i < patternLength - 3; i++)
        {
            if (pattern[i] == 'B' && pattern[i + 1] == 'B' &&
                pattern[i + 2] == 'B' && pattern[i + 3] == 'B')
            {
                bool leftOpen = (i > 0 && pattern[i - 1] == '0');
                bool rightOpen = (i + 4 < patternLength && pattern[i + 4] == '0');

                if (leftOpen || rightOpen)
                {
                    // 열린 4 위치 저장 (중복 방지)
                    if (!detectedFourPositions.Contains(i))
                    {
                        detectedFourPositions.Add(i);
                        fourPatternCount++;

                        if (fourPatternCount >= 2)
                            return true;
                    }
                }
            }
        }

        // 2. 변형된 열린 4 패턴 탐색 (B0BBB, BB0BB, BBB0B)
        string[] openFourPatterns = { "B0BBB", "BB0BB", "BBB0B" };

        for (int i = 0; i < patternLength - 4; i++)
        {
            foreach (string openFour in openFourPatterns)
            {
                if (pattern.Substring(i, 5) == openFour)
                {
                    bool leftOpen = (i > 0 && pattern[i - 1] == '0');
                    bool rightOpen = (i + 5 < patternLength && pattern[i + 5] == '0');

                    if (leftOpen || rightOpen)
                    {
                        if (!detectedFourPositions.Contains(i))
                        {
                            detectedFourPositions.Add(i);
                            fourPatternCount++;
                            if (fourPatternCount >= 2)
                                return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    
    // 오목 검사 함수
    private bool CheckFive(Vector2Int position)
    {
        SimulatePlacement(position, Constants.MarkerType.Black, out Constants.MarkerType originalValue);
        
        bool hasFive = false;
        Vector2Int[] directions = GetDirections();
        
        foreach (Vector2Int dir in directions)
        {
            int count = CountStonesInDirection(position, dir, Constants.MarkerType.Black, false);
            
            if (count == 5)
            {
                hasFive = true;
                Debug.Log($"Position {position}: Makes five in direction {dir}");
                break;
            }
        }
        
        _board[position.x, position.y] = originalValue;
        
        return hasFive;
    }
    
    // 장목 검사 함수
    private bool CheckOverline(Vector2Int position)
    {
        SimulatePlacement(position, Constants.MarkerType.Black, out Constants.MarkerType originalValue);
        
        bool hasOverline = false;
        Vector2Int[] directions = GetDirections();
        
        foreach (Vector2Int dir in directions)
        {
            int count = CountStonesInDirection(position, dir, Constants.MarkerType.Black, false);
            
            if (count > 5)
            {
                hasOverline = true;
                Debug.Log($"Position {position}: Creates overline ({count} stones) in direction {dir}");
                break;
            }
        }
        
        _board[position.x, position.y] = originalValue;
        
        return hasOverline;
    }
    
    // 미래에 장목이 형성되는지 확인
    private bool CheckFutureSixInRow(Vector2Int position)
    {
        SimulatePlacement(position, Constants.MarkerType.Black, out Constants.MarkerType originalValue);
        
        bool willFormSixInRow = false;
        Vector2Int[] directions = GetDirections();
        
        foreach (Vector2Int dir in directions)
        {
            string pattern = ExtractPattern(position, dir, 15);
            
            // 패턴에서 흑돌 'B'와 빈 공간 '0'을 모두 'P'로 변환 (Potential stones)
            string potentialPattern = "";
            foreach (char c in pattern)
            {
                potentialPattern += (c == BLACK || c == EMPTY) ? 'P' : c;
            }
            
            // 잠재적 연속 돌 개수 계산
            int maxConsecutive = 0;
            int current = 0;
            
            for (int i = 0; i < potentialPattern.Length; i++)
            {
                if (potentialPattern[i] == 'P')
                {
                    current++;
                    maxConsecutive = Math.Max(maxConsecutive, current);
                }
                else
                {
                    current = 0;
                }
            }
            
            // 연속 6개 이상의 잠재적 돌이 있고, 실제 패턴에 빈 공간이 포함되어 있으면 미래에 장목 형성 가능
            if (maxConsecutive >= 6 && pattern.Contains(EMPTY))
            {
                // 실제 빈 공간의 개수 확인 (너무 많으면 현실적이지 않음)
                int emptyCount = 0;
                foreach (char c in pattern)
                {
                    if (c == EMPTY)
                        emptyCount++;
                }
                
                // 빈 공간이 적당히 있을 때만 미래 장목으로 간주 (최대 3개)
                if (emptyCount <= 3)
                {
                    willFormSixInRow = true;
                    Debug.Log($"Position {position}: Will form six-in-a-row in future with {emptyCount} empty spaces in direction {dir}");
                    break;
                }
            }
        }
        
        _board[position.x, position.y] = originalValue;
        
        return willFormSixInRow;
    }
    
    // 상대방의 4목을 막는지 확인하는 함수
    private bool CheckBlockingFour(Vector2Int position)
    {
        SimulatePlacement(position, Constants.MarkerType.White, out Constants.MarkerType originalValue);
        
        bool blocksFour = false;
        Vector2Int[] directions = GetDirections();
        
        foreach (Vector2Int dir in directions)
        {
            int count = CountStonesInDirection(position, dir, Constants.MarkerType.White, false);
            
            if (count >= 4)
            {
                blocksFour = true;
                Debug.Log($"Position {position}: Blocks opponent's four in direction {dir}");
                break;
            }
        }
        
        _board[position.x, position.y] = originalValue;
        
        return blocksFour;
    }
    
    // 특정 방향으로 연속된 돌 개수 세기
    private int CountStonesInDirection(Vector2Int position, Vector2Int direction, Constants.MarkerType stoneType, bool allowGap)
    {
        int count = 1; // 자기 자신
        bool gapUsed = false;
        
        // 정방향 탐색
        Vector2Int currentPos = position + direction;
        while (IsInBoardRange(currentPos))
        {
            if (_board[currentPos.x, currentPos.y] == stoneType)
            {
                count++;
                currentPos += direction;
            }
            else if (allowGap && _board[currentPos.x, currentPos.y] == Constants.MarkerType.None && !gapUsed)
            {
                // 공백 허용 옵션이 켜져 있을 때만 처리
                gapUsed = true;
                Vector2Int nextPos = currentPos + direction;
                
                if (IsInBoardRange(nextPos) && _board[nextPos.x, nextPos.y] == stoneType)
                {
                    currentPos = nextPos;
                    continue;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        // 반대 방향 탐색
        gapUsed = false;
        currentPos = position - direction;
        
        while (IsInBoardRange(currentPos))
        {
            if (_board[currentPos.x, currentPos.y] == stoneType)
            {
                count++;
                currentPos -= direction;
            }
            else if (allowGap && _board[currentPos.x, currentPos.y] == Constants.MarkerType.None && !gapUsed)
            {
                gapUsed = true;
                Vector2Int nextPos = currentPos - direction;
                
                if (IsInBoardRange(nextPos) && _board[nextPos.x, nextPos.y] == stoneType)
                {
                    currentPos = nextPos;
                    continue;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        return count;
    }
    
    // 특정 위치가 금수인지 확인
    public bool IsForbiddenPosition(Vector2Int position)
    {
        // 흑돌 차례가 아니면 금수 규칙 적용 안함
        if (!_turnManager.IsBlackPlayerTurn())
            return false;
            
        return _forbiddenPositions[position.x, position.y];
    }
    
    // 금수 위치 배열 반환 (UI 표시용)
    public bool[,] GetForbiddenPositions()
    {
        return _forbiddenPositions;
    }
    
    // 보드 범위 내에 있는지 확인
    private bool IsInBoardRange(Vector2Int position)
    {
        return position.x >= 0 && position.x < _board.GetLength(0) &&
               position.y >= 0 && position.y < _board.GetLength(1);
    }
    
    // 헬퍼 함수: 임시로 돌 놓기
    private void SimulatePlacement(Vector2Int position, Constants.MarkerType stoneType, out Constants.MarkerType originalValue)
    {
        originalValue = _board[position.x, position.y];
        _board[position.x, position.y] = stoneType;
    }
    
    // 헬퍼 함수: 검사 방향 배열 반환
    private Vector2Int[] GetDirections()
    {
        return new Vector2Int[]
        {
            new Vector2Int(1, 0),   // 가로
            new Vector2Int(0, 1),   // 세로
            new Vector2Int(1, 1),   // 대각선 (↗)
            new Vector2Int(1, -1)   // 대각선 (↘)
        };
    }
    
    // 헬퍼 함수: 모든 방향에서 특정 패턴 조건 확인
    private bool CheckAnyDirectionPattern(Vector2Int position, Func<string, bool> patternChecker)
    {
        Vector2Int[] directions = GetDirections();
    
        foreach (Vector2Int dir in directions)
        {
            string pattern = ExtractPattern(position, dir, 11);
            if (patternChecker(pattern))
                return true;
        }
    
        return false;
    }

// 헬퍼 함수: 패턴 조건을 만족하는 방향 개수 세기
    private int CountPatternDirections(Vector2Int position, Func<string, bool> patternChecker)
    {
        Vector2Int[] directions = GetDirections();
        int count = 0;
    
        foreach (Vector2Int dir in directions)
        {
            string pattern = ExtractPattern(position, dir, 11);
            if (patternChecker(pattern))
                count++;
        }
    
        return count;
    }
}