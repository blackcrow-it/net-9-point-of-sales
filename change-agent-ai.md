# Hướng dẫn đổi Agent AI của GitHub Spec Kit

## Trường hợp chưa có Agent khác
Chạy lệnh tạo agent mới
```sh
# Initialize with specific AI assistant
specify init . --ai claude
```

## Cập nhật file context của Agent đó
Có thể sử dụng promt để AI tự cập nhật từ Agent trước sang Agent mới chuyển

## Cập nhật file plan
Trong file `.specify\templates\plan-template.md` tại phần `Update agent file incrementally` cập nhật nội dung:
```text
Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude`
```