import http from 'k6/http';
import { sleep } from 'k6';

// URL ของ API (แก้เป็น localhost ของคุณ)
const baseUrl = 'https://localhost:7233'; 

const testRunId = `run_${Date.now()}`;

export const options = {
  scenarios: {
    // Scenario ที่ 1: ยิง API /deposit 10 ครั้ง
    deposit_test: {
      executor: 'per-vu-iterations', // ยิงตามจำนวนครั้งที่กำหนด
      exec: 'deposit',                // เรียก function 'deposit'
      vus: 1,                         // ใช้ 1 Virtual User
      iterations: 100,                 // ให้ user นี้ยิง 10 ครั้ง
      startTime: '0s',                // เริ่มทันที
    },
    // Scenario ที่ 2: ยิง API /withdraw 10 ครั้ง
    withdraw_test: {
      executor: 'per-vu-iterations',
      exec: 'withdraw',
      vus: 1,
      iterations: 100,
      startTime: '2s', // (เริ่มช้ากว่าตัวแรก 2 วิ จะได้ไม่ปนกัน)
    },
    // Scenario ที่ 3: ยิง API /transfer 10 ครั้ง
    transfer_test: {
      executor: 'per-vu-iterations',
      exec: 'transfer',
      vus: 1,
      iterations: 100,
      startTime: '4s', // (เริ่มช้ากว่าตัวที่สอง 2 วิ)
    },
  },
};

const headers = { 'Content-Type': 'application/json' };

// Function สำหรับยิง /deposit
export function deposit() {
  // สร้าง Running Number ที่ไม่ซ้ำกัน (เช่น deposit_iter_0, deposit_iter_1)
  const runningNumber = `deposit_iter_${__ITER}`;

  const payload = JSON.stringify({
    userId: `user_${runningNumber}`,
    amount: 100,
    runningNumber: runningNumber,
    testRunId: testRunId
  });

  http.post(`${baseUrl}/transaction/deposit`, payload, { headers });
  sleep(0.5); // (พักครึ่งวิ)
}

// Function สำหรับยิง /withdraw
export function withdraw() {
  const runningNumber = `withdraw_iter_${__ITER}`;

  const payload = JSON.stringify({
    userId: `user_${runningNumber}`,
    amount: 200,
    runningNumber: runningNumber,
    testRunId: testRunId
  });

  http.post(`${baseUrl}/transaction/withdraw`, payload, { headers });
  sleep(0.5);
}

// Function สำหรับยิง /transfer
export function transfer() {
  const runningNumber = `transfer_iter_${__ITER}`;

  const payload = JSON.stringify({
    userId: `user_${runningNumber}`,
    amount: 300,
    runningNumber: runningNumber,
    testRunId: testRunId
  });

  http.post(`${baseUrl}/transaction/transfer`, payload, { headers });
  sleep(0.5);
}